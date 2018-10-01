using System;
using System.Media;
using System.Threading.Tasks;
using HdSplit.Properties;

namespace HdSplit.Models
{
    public static class Sounds
    {
        public static bool soundFinished = true;

        public static void PlayScanSound()
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    SoundPlayer notificationSound = new SoundPlayer(HdSplit.Properties.Resources.scanSound);
                    notificationSound.Play();

                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
    }
}