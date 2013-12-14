using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

		/// <summary>
		/// Gets if the node is at the root.
		/// </summary>
		public bool IsRoot
		{
			get { return this.Parent == null; }
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
			get { return this.Depth == _maxDepth; }
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
			get { return this.Childs != null; }
		}

		/// <summary>
		/// Gets if the node as disabled childs.
		/// </summary>
		public bool HasDisabledChilds
		{
			get { return this.Childs.FirstOrDefault(e => e.IsEnabled == false) != null; }
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
			this.Parent = parent;
			this.Depth = parent != null ? parent.Depth + 1 : 0;
			_maxDepth = maxDepth;
			_boundingBox = boundingBox;

			this.IsEnabled = enabled;
		}

		/// <summary>
		/// This method collects the geometry used for drawing the node.
		/// </summary>
		/// <param name="vertices">The vertices.</param>
		/// <param name="indices">The indices.</param>
		/// <param name="state">True to only collect if enabled, false if to collect when disabled.</param>
		public void CollectGeometry(ICollection<Vector2> vertices, ICollection<ushort> indices, bool state = true)
		{
			var childs = this.Childs;
			if (childs != null)
			{
				for (int i = 0; i < 4; ++i)
				{
					var child = childs[i];
					child.CollectGeometry(vertices, indices, state);
				}
			}
			else if (this.IsEnabled == state)
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

		/// <summary>
		/// Changes the state of the intersected quads.
		/// </summary>
		/// <param name="brush">The brush.</param>
		/// <param name="state">The state, either enabled or disabled.</param>
		/// <returns>True if any quads were changed.</returns>
		public bool SetQuads(ITerrainBrush brush, bool state)
		{
			bool changed = false;

			if (this.IsEnabled != state || this.HasChilds == true)
			{
				var containment = brush.Contains(ref _boundingBox);
				if (containment == ContainmentType.Contains)
				{
					this.IsEnabled = state;
					this.Childs = null; // <-- Remove any childs.
					changed = true;
				}
				else if (containment == ContainmentType.Intersects)
				{
					// If we only intersect and are not contained by the brush, we need to recurse down
					// until we have reached the maximum depth, or have found a child which is contained.

					// Create childs if necessary.
					if (this.Childs == null && this.Depth < _maxDepth)
					{
						var min = _boundingBox.Min;
						var max = _boundingBox.Max;
						var center = min + ((max - min) / 2.0f);

						this.Childs = new QuadTreeNode[]
						{
							new QuadTreeNode(this, _maxDepth, new BoundingBox(min, center), this.IsEnabled),
							new QuadTreeNode(this, _maxDepth, new BoundingBox(new Vector3(center.X, min.Y, 0.0f), new Vector3(max.X, center.Y, 0.0f)), this.IsEnabled),
							new QuadTreeNode(this, _maxDepth, new BoundingBox(new Vector3(min.X, center.Y, 0.0f), new Vector3(center.X, max.Y, 0.0f)), this.IsEnabled),
							new QuadTreeNode(this, _maxDepth, new BoundingBox(center, max), this.IsEnabled)
						};
					}

					// Recurse down.
					if (this.Childs != null)
					{
						// Try to change the state of our child quads.
						bool canCollapse = true;
						foreach (var child in this.Childs)
						{
							if (child.SetQuads(brush, state) == true)
							{
								changed = true;
							}

							// We can't collapse if any of our child also has childs,
							// or if one of our childs has a different state.
							if (child.IsEnabled != state || child.HasChilds == true)
							{
								canCollapse = false;
							}
						}

						if (canCollapse == true)
						{
							// All childs have the same correct state, so simplify.
							this.IsEnabled = state;
							this.Childs = null;
						}
						else
						{
							// Not all childs have the same state, so we have to be enabled.
							this.IsEnabled = true;
						}
					}
					else
					{
						// We can't recure down because we are at the maximum depth,
						// so handle as if we were "contained".
						this.IsEnabled = state;
						changed = true;
					}
				}
			}

			return changed;
		}
	}
}
