using System;
using System.IO;
using System.Windows.Media;

namespace RH.HeadShop.Render.Controllers
{
    /// <summary> Контроллер, ответственный за работу со звуком </summary>
    public class SoundController
    {
        private bool isPlaying;
        private MediaPlayer player;

        public SoundController()
        {
            player = new MediaPlayer();
        }
        ~SoundController()
        {
            player = null;
        }

        public void Play(string path)
        {
            if (!File.Exists(path) || isPlaying)
                return;

            isPlaying = true;

            player.Open(new Uri(path));
            player.Play();
        }
        public void Stop()
        {
            if (isPlaying)
            {
                isPlaying = false;
                player.Stop();
            }
        }
    }
}
