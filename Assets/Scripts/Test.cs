using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Scripts.RBTree;

namespace Scripts {

	/// <summary>
	/// 赤黒木の確認用クラス
	/// </summary>
	public class Test : MonoBehaviour{

		public Text text;

		private long insertSum = 0;
		private long deleteSum = 0;

		private IEnumerator Start() {
			RBTree<int> rbMap = new RBTree<int>();

			int[] nums = new int[100000];
			for (int i = 0; i < nums.Length; ++i) {
				nums[i] = i;
			}

			int counter = 0;

			while (true) {

				++counter;

				//追加
				var sw = System.Diagnostics.Stopwatch.StartNew();
				for (int i = 0; i < nums.Length; ++i) {
					rbMap.Insert(nums[i]);
				}
				sw.Stop();
				Debug.Log("Insert(" + nums.Length + ") is " + sw.ElapsedMilliseconds + " : Avg(" + counter + ") is " + (insertSum += sw.ElapsedMilliseconds) / counter);
				sw.Reset();
				
				//確認
				//Debug.Log(rbMap);

				//削除
				sw.Start();
				for (int i = 0; i < nums.Length; ++i) {
					rbMap.Delete(nums[i]);
				}
				sw.Stop();
				Debug.Log("Delete(" + nums.Length + ") is " + sw.ElapsedMilliseconds + " : Avg(" + counter + ") is " + (deleteSum += sw.ElapsedMilliseconds) / counter);
				Debug.Log(rbMap);

				yield return new WaitForSeconds(0.4f);
			}
		}
	}
}