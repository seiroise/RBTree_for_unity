using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scripts.RBTree {

	/// <summary>
	/// 赤黒木
	/// </summary>
	public class RBTree<K> where K : IComparable<K> {

		/// <summary>
		/// 赤黒ノード
		/// </summary>
		public class Node {
			public RBColor color;
			public K key;
			public Node lst = null;		//左部分木
			public Node rst = null;		//右部分木
			public Node parent = null;	//親

			public Node prev = null;	//一つ小さい値のkeyを持つNode
			public Node next = null;	//一つ大きい値のkeyを持つNode

			public Node(RBColor color, K key, Node parent) {
				this.color = color;
				this.key = key;
				this.parent = parent;
			}
		}

		/// <summary>
		/// 修正情報クラス
		/// </summary>
		private class Trio {
			public bool change = false;
			public K lmax;
		}

		private Node root;			//根
		private Node smallest;		//最も小さいノード
		private Node biggest;		//最も大きいノード

		#region Constructor

		public RBTree() {
			this.root = null;
		}

		#endregion

		#region PublicFunction

		/// <summary>
		/// keyの包含確認
		/// </summary>
		public bool ContainsKey(K key) {
			Node t = root;
			while (t != null) {
				int com = key.CompareTo(t.key);
				if (com < 0) t = t.lst;
				else if (com > 0) t = t.rst;
				else return true;
			}
			return false;
		}

		/// <summary>
		/// keyに対応するNodeの取得
		/// </summary>
		public Node GetNode(K key) {
			Node t = root;
			while (t != null) {
				int com = key.CompareTo(t.key);
				if (com < 0) t = t.lst;
				else if (com > 0) t = t.rst;
				else return t;
			}
			return null;
		}

		/// <summary>
		/// 空ならtrue、空でないならfalse
		/// </summary>
		public bool IsEmpty() {
			return root == null;
		}

		/// <summary>
		/// 空にする
		/// </summary>
		public void Clear() {
			root = null;
		}

		#endregion

		#region PrivateFunction

		/// <summary>
		/// ノードnが赤いか確認する
		/// </summary>
		private bool IsR(Node n) {
			return n != null && n.color == RBColor.R;
		}

		/// <summary>
		/// ノードnが黒いか確認する
		/// </summary>
		private bool IsB(Node n) {
			return n != null && n.color == RBColor.B;
		}

		/// <summary>
		/// ２分探索木ｖの左回転。回転した木を返す
		/// </summary>
		private Node RotateL(Node v) {

			//ノード関係
			Node u = v.rst, t = u.lst;
			u.lst = v;
			v.rst = t;

			//親子関係
			u.parent = v.parent;
			v.parent = u;
			if(t != null) t.parent = v;

			return u;
		}

		/// <summary>
		/// ２分探索木uの右回転。回転した木を返す
		/// </summary>
		private Node RotateR(Node u) {

			//ノード関係
			Node v = u.lst, t = v.rst;
			v.rst = u;
			u.lst = t;

			//親子関係
			v.parent = u.parent;
			u.parent = v;
			if (t != null) t.parent = u;

			return v;
		}

		/// <summary>
		/// ２分探索木tの二重回転(左->右回転)。回転した木を返す
		/// </summary>
		private Node RotateLR(Node t) {
			t.lst = RotateL(t.lst);
			return RotateR(t);
		}

		/// <summary>
		/// ２分探索木tの二重回転(右->左回転)。回転した木を返す
		/// </summary>
		private Node RotateRL(Node t) {
			t.rst = RotateR(t.rst);
			return RotateL(t);
		}

		#endregion

		#region InsertFunction

		/// <summary>
		/// エントリーの挿入
		/// </summary>
		public void Insert(K key) {
			root = Insert(root, null, key);
			root.color = RBColor.B;
		}

		/// <summary>
		/// 再帰的なエントリーの挿入操作
		/// </summary>
		private Node Insert(Node t, Node parent, K key) {
			if (t == null) {
				//nullなら赤ノードを返す
				return CreateNode(key, parent);
			} else {
				int com = key.CompareTo(t.key);
				if (com < 0) {
					//左(小さい方)に進む
					t.lst = Insert(t.lst, t, key);
					return Balance(t);
				} else if (com > 0) {
					//右(大きい方)に進む
					t.rst = Insert(t.rst, t, key);
					return Balance(t);
				} else {
					//同値
					return t;
				}
			}
		}

		/// <summary>
		/// エントリー挿入に伴う赤黒木の修正(4パターン
		/// </summary>
		private Node Balance(Node t) {
			if (t.color != RBColor.B) {
				return t;
			}
			if (IsR(t.lst)) {
				if (IsR(t.lst.lst)) {
					//左の子と左左の孫が赤ノード
					t = RotateR(t);
					t.lst.color = RBColor.B;
					return t;
				} else if (IsR(t.lst.rst)) {
					//左の子と左右の孫が赤ノード
					t = RotateLR(t);
					t.lst.color = RBColor.B;
					return t;
				}
			}
			if (IsR(t.rst)) {
				if (IsR(t.rst.lst)) {
					//右の子と右左の孫が赤ノード
					t = RotateRL(t);
					t.rst.color = RBColor.B;
					return t;
				} else if (IsR(t.rst.rst)) {
					//右の子と右右の孫が赤ノード
					t = RotateL(t);
					t.rst.color = RBColor.B;
					return t;
				}
			}
			return t;
		}

		/// <summary>
		/// 新しくエントリーのノードを作成する
		/// </summary>
		private Node CreateNode(K key, Node parent) {
			Node t = new Node(RBColor.R, key, parent);
			
			//前後関係の構築
			if (parent == null) {
				//rootの場合
				smallest = biggest = t;
			} else {
				//それ以外
				if (!SetPrevAtInserted(t) && t != biggest) {
					SetNextAtInserted(t);
				}
			}
			return t;
		}

		/// <summary>
		/// 挿入するノードtに対してkeyが一つ前のノードnとの前後関係を設定する
		/// 前後関係をnの次のノードn.nextとも構築した場合はtrueを返す
		/// </summary>
		private bool SetPrevAtInserted(Node t) {

			//親に遡って最初に見つけたノードtよりも小さいノード
			Node n = t.parent;
			int com;
			while (n != null) {
				com = n.key.CompareTo(t.key);
				if (com < 0) {
					//前後関係の構築
					t.prev = n;
					if (n.next != null) {
						n.next.prev = t;
						t.next = n.next;
						n.next = t;
						return true;
					} else {
						n.next = t;
						break;
					}
				} else {
					n = n.parent;
				}
			}

			//最大ノード
			biggest = t;
			return false;
		}

		/// <summary>
		/// 挿入するノードtに対してkeyが一つ次のノードnとの前後関係を設定する
		/// 前後関係をnの前のノードn.prevとも構築した場合はtrueを返す
		/// </summary>
		private bool SetNextAtInserted(Node t) {

			//親に遡って最初に見つけたノードtよりも大きいノード
			Node n = t.parent;
			int com;
			while (n != null) {
				com = n.key.CompareTo(t.key);
				if (com > 0) {
					//前後関係の構築
					t.next = n;
					if (n.prev != null) {
						n.prev.next = t;
						t.prev = n.prev;
						n.prev = t;
						return true;
					} else {
						n.prev = t;
						break;
					}
				} else {
					n = n.parent;
				}
			}

			//最小ノード
			smallest = t;
			return false;
		}

		#endregion

		#region DeleteFunction

		/// <summary>
		/// keyのエントリーの削除
		/// </summary>
		public void Delete(K key) {
			if (root == null) return;
			root = Delete(root, null, key, new Trio());
			if (root != null) root.color = RBColor.B;
		}

		/// <summary>
		/// 再帰的なkeyのエントリーの削除操作
		/// </summary>
		private Node Delete(Node t, Node parent, K key, Trio aux) {
			if (t == null) {
				aux.change = false;
				return null;
			} else if (key.CompareTo(t.key) < 0) {
				//左(小さい方)に進む
				t.lst = Delete(t.lst, t, key, aux);
				return BalanceL(t, aux);
			} else if (key.CompareTo(t.key) > 0) {
				//右(大きい方)に進む
				t.rst = Delete(t.rst, t, key, aux);
				return BalanceR(t, aux);
			} else {
				if (t.lst == null) {
					//左端
					switch (t.color) {
						case RBColor.R:
							aux.change = false;
							break;
						case RBColor.B:
							//パス上の黒の数が変わるので修正フラグを立てる
							aux.change = true;
							break;
					}
					//親子関係
					if(t.rst != null) {
						t.rst.parent = t.parent;
					}
					//前後関係
					SetPrevNextAtDeleted(t);
					//右部分木を昇格する
					return t.rst;
				} else {
					//前後関係の設定
					SetPrevNextAtDeleted(t);
					//左部分木の最大値で置き換える
					t.lst = DeleteMax(t.lst, aux);
					t.key = aux.lmax;
					return BalanceL(t, aux);
				}
			}
		}

		/// <summary>
		/// 左部分木の最大値のノードを削除しauxに格納する
		/// 戻り値は削除により修正された左部分木
		/// </summary>
		private Node DeleteMax(Node t, Trio aux) {
			if (t.rst == null) {
				//最大値
				aux.lmax = t.key;
				switch (t.color) {
					case RBColor.R:
						aux.change = false;
						break;
					case RBColor.B:
						//パス上の黒の数が変わるので修正フラグを立てる
						aux.change = true;
						break;
				}
				//親子関係
				if (t.lst != null) {
					t.lst.parent = t.parent;
				}
				//左部分木を昇格させる
				return t.lst;
			} else {
				//最大値に向かって進む
				t.rst = DeleteMax(t.rst, aux);
				return BalanceR(t, aux);
			}
		}

		/// <summary>
		/// 左部分木のエントリー削除に伴う赤黒木の修正(4つのパターン
		/// 戻り値は修正された木。auxに付加情報を返す
		/// </summary>
		private Node BalanceL(Node t, Trio aux) {
			if (!aux.change) {
				return t;
			} else if (IsB(t.rst) && IsR(t.rst.lst)) {
				RBColor rb = t.color;
				t = RotateRL(t);
				t.color = rb;
				t.lst.color = RBColor.B;	//左側に黒が追加されて修正
				aux.change = false;
			} else if (IsB(t.rst) && IsR(t.rst.rst)) {
				RBColor rb = t.color;
				t = RotateL(t);
				t.color = rb;
				t.lst.color = t.rst.color = RBColor.B;	//左側に黒が追加されて修正
				aux.change = false;
			} else if (IsB(t.rst)) {
				RBColor rb = t.color;
				t.color = RBColor.B;
				t.rst.color = RBColor.R;
				aux.change = rb == RBColor.B;	//tが黒の場合は黒が減るので修正フラグを立て木を遡る(親で何とかする
			} else if (IsR(t.rst)) {
				t = RotateL(t);
				t.color = RBColor.B;
				t.lst.color = RBColor.R;
				t.lst = BalanceL(t.lst, aux);	//再帰は必ず一段で終わる
				aux.change = false;
			} else {
				throw new Exception("(L) This program is buggy");
			}
			return t;
		}

		/// <summary>
		/// 右部分木のエントリー削除に伴う赤黒木の修正(4つのパターン
		/// 戻り値は修正された木。auzに付加情報を返す
		/// </summary>
		private Node BalanceR(Node t, Trio aux) {
			//BalanceLと左右対称
			if (!aux.change) {
				return t;
			} else if (IsB(t.lst) && IsR(t.lst.rst)) {
				RBColor rb = t.color;
				t = RotateLR(t);
				t.color = rb;
				t.rst.color = RBColor.B;
				aux.change = false;
			} else if (IsB(t.lst) && IsR(t.lst.lst)) {
				RBColor rb = t.color;
				t = RotateR(t);
				t.color = rb;
				t.lst.color = t.rst.color = RBColor.B;
				aux.change = false;
			} else if (IsB(t.lst)) {
				RBColor rb = t.color;
				t.color = RBColor.B;
				t.lst.color = RBColor.R;
				aux.change = rb == RBColor.B;
			} else if (IsR(t.lst)) {
				t = RotateR(t);
				t.color = RBColor.B;
				t.rst.color = RBColor.R;
				t.rst = BalanceR(t.rst, aux);
				aux.change = false;
			} else {
				throw new Exception("(L) This program is buggy");
			}
			return t;
		}

		/// <summary>
		/// 削除するノードtに対しての前後関係を設定する
		/// </summary>
		private void SetPrevNextAtDeleted(Node t) {
			//前後関係
			if (t.prev != null) {
				t.prev.next = t.next;
			} else {
				smallest = t.next;
			}
			if (t.next != null) {
				t.next.prev = t.prev;
			} else {
				biggest = t.prev;
			}
		}

		#endregion

		#region VirtualFunction

		public override string ToString() {
			return ToString("s");
		}

		public string ToString(string format) {
			
			//空
			if (IsEmpty()) return "Empty Tree";

			StringBuilder sb = new StringBuilder();
			if (format.Equals("b")) {
				//大きい順
				Node t = biggest;
				sb.Append(t.key);
				t = t.prev;
				while (t != null) {
					sb.Append(" -> ");
					sb.Append(t.key);
					t = t.prev;
				}
			} else if (format.Equals("s")) {
				//小さい順
				Node t = smallest;
				sb.Append(t.key);
				t = t.next;
				while (t != null) {
					sb.Append(" -> ");
					sb.Append(t.key);
					t = t.next;
				}
			}

			return sb.ToString();

		}

		#endregion
	}
}