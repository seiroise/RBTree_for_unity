using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scripts.RBTree {
	
	/// <summary>
	/// 赤黒ノード
	/// </summary>
	public class RBNode<K, V> where K : IComparable<K> {
		public RBColor color;
		public K key;
		public V value;
		public RBNode<K, V> lst = null;
		public RBNode<K, V> rst = null;

		public RBNode(RBColor color, K key, V value) {
			this.color = color;
			this.key = key;
			this.value = value;
		}
	}
}