using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scripts.RBTree {

	/// <summary>
	/// 赤黒木_beta1
	/// </summary>
	public class RBTree_b1<K, V> where K : IComparable<K> {

		/// <summary>
		/// 赤黒ノード
		/// </summary>
		private class Node {
			public RBColor color;
			public K key;
			public V value;
			public Node lst = null;
			public Node rst = null;

			public Node(RBColor color, K key, V value) {
				this.color = color;
				this.key = key;
				this.value = value;
			}
		}

		/// <summary>
		/// 修正情報クラス
		/// </summary>
		private class Trio {
			public bool change;	//修正が必要か
			public K lmax;		//左部分着のキーの最大値
			public V value;		//lmaxに対応する値
		}

		private Node root = null;	//根

		#region PublicFunction

		/// <summary>
		/// keyの包含確認
		/// </summary>
		public bool ContainsKey(K key) {
			Node t = root;
			while (t != null) {
				int com = key.CompareTo(t.key);
				if (com < 0) {
					t = t.lst;
				} else if (com > 0) {
					t = t.rst;
				} else {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// keyから値の取得
		/// </summary>
		public V GetValue(K key) {
			Node t = root;
			while (t != null) {
				int com = key.CompareTo(t.key);
				if (com < 0) {
					t = t.lst;
				} else if (com > 0) {
					t = t.rst;
				} else {
					return t.value;
				}
			}
			return default(V);
		}

		/// <summary>
		/// 空ならtrue、空出ないならfalse
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
			Node u = v.rst, t = u.lst;
			u.lst = v;
			v.rst = t;
			return u;
		}

		/// <summary>
		/// ２分探索木uの右回転。回転した木を返す
		/// </summary>
		private Node RotateR(Node u) {
			Node v = u.lst, t = v.rst;
			v.rst = u;
			u.lst = t;
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
		public void Insert(K key, V value) {
			root = Insert(root, key, value);
			root.color = RBColor.B;
		}

		/// <summary>
		/// 再帰的なエントリーの挿入操作
		/// </summary>
		private Node Insert(Node t, K key, V value) {
			if (t == null) {
				//nullなら赤ノードを返す
				return new Node(RBColor.R, key, value);
			} else {
				int com = key.CompareTo(t.key);
				if (com < 0) {
					//左(小さい方)に進む
					t.lst = Insert(t.lst, key, value);
					return Balance(t);
				} else if (com > 0) {
					//右(大きい方)に進む
					t.rst = Insert(t.rst, key, value);
					return Balance(t);
				} else {
					//同値
					t.value = value;
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

		#endregion

		#region DeleteFunction

		/// <summary>
		/// keyのエントリーの削除
		/// </summary>
		public void Delete(K key) {
			if (root == null) return;
			root = Delete(root, key, new Trio());
			if (root != null) root.color = RBColor.B;
		}

		/// <summary>
		/// 再帰的なkeyのエントリーの削除操作
		/// </summary>
		private Node Delete(Node t, K key, Trio aux) {
			if (t == null) {
				aux.change = false;
				return null;
			} else if (key.CompareTo(t.key) < 0) {
				//左(小さい方)に進む
				t.lst = Delete(t.lst, key, aux);
				return BalanceL(t, aux);
			} else if (key.CompareTo(t.key) > 0) {
				//右(大きい方)に進む
				t.rst = Delete(t.rst, key, aux);
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
					//右部分木を昇格する
					return t.rst;
				} else {
					//左部分木の最大値で置き換える
					t.lst = DeleteMax(t.lst, aux);
					t.key = aux.lmax;
					t.value = aux.value;
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
				aux.value = t.value;
				switch (t.color) {
					case RBColor.R:
						aux.change = false;
						break;
					case RBColor.B:
						//パス上の黒の数が変わるので修正フラグを立てる
						aux.change = true;
						break;
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

		#endregion

		#region DebugFunction

		/// <summary>
		/// 赤黒木をグラフ文字列に変換する
		/// </summary>
		public override string ToString() {
			Debug.Log("Tree");
			ToString(root);
			return "";

		}

		private void ToString(Node t) {
			if (t != null) {
				ToString(t.lst);
				Debug.Log(t.color + ":" + t.key);
				ToString(t.rst);
			}
		}

		#endregion

	}
}