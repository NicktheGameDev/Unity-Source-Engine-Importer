
using UnityEngine;

namespace uSource
{
    // Main menu background loader
    public class MainMenuBackgrounds : MonoBehaviour
    {
        public enum Mode { StaticImage, ThreeDScene }
        public Mode mode = Mode.ThreeDScene;
        public string sceneName;
        public Texture2D staticImage;

        void Start()
        {
            if(mode == Mode.StaticImage)
            {
                var go = new GameObject("MainMenuBG");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = Sprite.Create(staticImage, new Rect(0,0,staticImage.width,staticImage.height), Vector2.one*0.5f);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            }
        }
    }
}
