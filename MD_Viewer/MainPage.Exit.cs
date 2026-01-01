using System;

namespace MD_Viewer
{
		public partial class MainPage
		{
			/// <summary>
			/// 離開按鈕點擊：統一走頁面的離開流程（OnDisappearing 會負責詢問是否儲存）
			/// </summary>
			private void OnExitClicked(object? sender, EventArgs e)
			{
				// 觸發應用程式結束；實際「是否儲存」的詢問在 MainPage.OnDisappearing 中統一處理
				Application.Current?.Quit();
			}
		}
}

