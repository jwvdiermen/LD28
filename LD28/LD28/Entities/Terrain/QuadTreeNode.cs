using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace LD28.Entities.Terrain
{
	/// <summary>
	/// This class represents a node in a <see cref="QuadTree" />.
	/// </summary>
	public class QuadTreeNode
	{
		private readonly int _maxDepth;
		private BoundingBox _boundingBox;
		private Fixture _fixture;

		/// <summary>
		/// Gets if the node is at the root.
		/// </summary>
		public bool IsRoot
		{
			get { return Parent == null; }
		}

		/// <summary>
		/// Gets the parent.
		/// </summary>
		public QuadTreeNode Parent
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the depth.
		/// </summary>
		public int Depth
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets if the node is a leaf, meaning it is at the maximum depth.
		/// </summary>
		public bool IsLeaf
		{
			get { return Depth == _maxDepth; }
		}

		/// <summary>
		/// Gets the offset of the node in relation to the root.
		/// </summary>
		public BoundingBox BoundingBox
		{
			get { return _boundingBox; }
		}

		/// <summary>
		/// Gets if the node is enabled.
		/// </summary>
		public bool IsEnabled { get; private set; }

		/// <summary>
		/// Gets if the node has childs.
		/// </summary>
		public bool HasChilds
		{
			get { return Childs != null; }
		}

		/// <summary>
		/// Gets if the node as disabled childs.
		/// </summary>
		public bool HasDisabledChilds
		{
			get { return Childs.FirstOrDefault(e => e.IsEnabled == false) != null; }
		}

		/// <summary>
		/// Gets an array containing the childs.
		/// </summary>
		public QuadTreeNode[] Childs
		{
			get;
			private set;
		}

		internal QuadTreeNode(QuadTreeNode parent, int maxDepth, BoundingBox boundingBox, bool enabled)
		{
			Parent = parent;
			Depth = parent != null ? parent.Depth + 1 : 0;
			_maxDepth = maxDepth;
			_boundingBox = boundingBox;

			IsEnabled = enabled;
		}

		/// <summary>
		/// This method collects the geometry used for drawing the node.
		/// </summary>
		/// <param name="vertices">The vertices.</param>
		/// <param name="indices">The indices.</param>
		/// <param name="state">True to only collect if enabled, false if to collect when disabled.</param>
		public void CollectGeometry(ICollection<Vector2> vertices, ICollection<ushort> indices, bool state = true)
		{
			var childs = Childs;
			if (childs != null)
			{
				for (var i = 0; i < 4; ++i)
				{
					var child = childs[i];
					child.CollectGeometry(vertices, indices, state);
				}
			}
			else if (IsEnabled == state)
			{
				var min = _boundingBox.Min;
				var max = _boundingBox.Max;

				var indexOffset = (ushort)vertices.Count;

				vertices.Add(new Vector2(min.X, min.Y));
				vertices.Add(new Vector2(max.X, min.Y));
				vertices.Add(new Vector2(max.X, max.Y));
				vertices.Add(new Vector2(min.X, max.Y));

				indices.Add((ushort)(indexOffset + 0));
				indices.Add((ushort)(indexOffset + 1));
				indices.Add((ushort)(indexOffset + 2));
				indices.Add((ushort)(indexOffset + 3));
				indices.Add((ushort)(indexOffset + 0));
				indices.Add((ushort)(indexOffset + 2));
			}
		}

		internal void UpdateFixtures(Body body)
		{
			if ((!IsEnabled || HasChilds) && _fixture != null)
			{
				_fixture.Body.DestroyFixture(_fixture);
				_fixture = null;
			}

			if (HasChilds)
			{
				foreach (var child in Childs)
				{
					if (child.IsEnabled)
					{
						child.UpdateFixtures(body);
					}
				}
			}
			else if (IsEnabled && _fixture == null)
			{
				var min = _boundingBox.Min;
				var size = _boundingBox.Max - min;
				var halfSize = size / 2.0f;
				var offset = new Vector2(min.X + halfSize.X, min.Y + halfSize.Y);
				
				_fixture = FixtureFactory.AttachRectangle(size.X, size.Y, 0.0f, offset, body);
				_fixture.CollisionCategories = Category.Cat2;
			}
		}

		internal void DestroyFixtures()
		{
			if (_fixture != null)
			{
				_fixture.Body.DestroyFixture(_fixture);
				_fixture = null;
			}
			
			if (HasChilds)
			{
				foreach (var child in Childs)
				{
					if (child.IsEnabled)
					{
						child.DestroyFixtures();
					}
				}
			}
		}

		/// <summary>
		/// Changes the state of the intersected quads.
		/// </summary>
		/// <param name="brush">The brush.</param>
		/// <param name="offset">The offset.</param>
		/// <param name="state">The state, either enabled or disabled.</param>
		/// <returns>True if any quads were changed.</returns>
		public bool SetQuads(ITerrainBrush brush, Vector2 offset, bool state)
		{
			var changed = false;

			var boundingBox = new BoundingBox(
				_boundingBox.Min + new Vector3(offset, 0.0f), 
				_boundingBox.Max + new Vector3(offset, 0.0f));

			if (IsEnabled != state || HasChilds)
			{
				var containment = brush.Contains(ref boundingBox);
				if (containment == ContainmentType.Contains)
				{
					IsEnabled = state;
					if (!IsEnabled)
					{
						DestroyFixtures();
					}

					Childs = null;
					changed = true;
				}
				else if (containment == ContainmentType.Intersects)
				{
					// If we only intersect and are not contained by the brush, we need to recurse down
					// until we have reached the maximum depth, or have found a child which is contained.

					// Create childs if necessary.
					if (Childs == null && Depth < _maxDepth)
					{
						var min = _boundingBox.Min;
						var max = _boundingBox.Max;
						var center = min + ((max - min) / 2.0f);

						Childs = new[]
						{
							new QuadTreeNode(this, _maxDepth, new BoundingBox(min, center), IsEnabled),
							new QuadTreeNode(this, _maxDepth, new BoundingBox(new Vector3(center.X, min.Y, 0.0f), new Vector3(max.X, center.Y, 0.0f)), IsEnabled),
							new QuadTreeNode(this, _maxDepth, new BoundingBox(new Vector3(min.X, center.Y, 0.0f), new Vector3(center.X, max.Y, 0.0f)), IsEnabled),
							new QuadTreeNode(this, _maxDepth, new BoundingBox(center, max), IsEnabled)
						};

						changed = true;
					}

					// Recurse down.
					if (Childs != null)
					{
						// Try to change the state of our child quads.
						var canCollapse = true;
						foreach (var child in Childs)
						{
							if (child.SetQuads(brush, offset, state))
							{
								changed = true;
							}

							// We can't collapse if any of our child also has childs,
							// or if one of our childs has a different state.
							if (child.IsEnabled != state || child.HasChilds)
							{
								canCollapse = false;
							}
						}

						if (canCollapse)
						{
							// All childs have the same correct state, so simplify.
							IsEnabled = state;
							if (!IsEnabled)
							{
								DestroyFixtures();
							}

							foreach (var child in Childs)
							{
								child.DestroyFixtures();
							}
							Childs = null;
						}
						else
						{
							// Not all childs have the same state, so we have to be enabled.
							IsEnabled = true;
						}
					}
					else
					{
						// We can't recure down because we are at the maximum depth,
						// so handle as if we were "contained".
						IsEnabled = state;
						if (!IsEnabled)
						{
							DestroyFixtures();
						}

						changed = true;
					}
				}
			}

			return changed;
		}
	}
}
