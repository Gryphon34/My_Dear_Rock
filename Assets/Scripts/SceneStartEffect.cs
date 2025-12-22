using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class SceneStartEffect : MonoBehaviour
{
    public CanvasGroup fadeGroup;

    async void Start()
    {
        if (fadeGroup != null)
        {
            fadeGroup.alpha = 1f;
            float time = 0;
            while (time < 1.0f)
            {
                time += Time.deltaTime;
                fadeGroup.alpha = Mathf.Lerp(1f, 0f, time / 1.0f);
                await Task.Yield();
            }
            fadeGroup.alpha = 0f;
            gameObject.SetActive(false); // 효과 완료 후 비활성화
        }
    }
}