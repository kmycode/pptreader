using PptReader.Models.Cloud.Speech;
using PptReader.Models.Common;
using PptReader.Models.Office;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace PptReader.Models.Sound
{
    class SpeakModel
    {
        private SlideShow slideshow;
        private SoundDataDirectory dic;
        private Synthesize cloud;

        private string lastPhrase;
        
        public SpeakModel(SlideShow slideshow)
        {
            this.slideshow = slideshow;

            this.cloud = new Synthesize();

            if (!Directory.Exists(".\\sound"))
            {
                Directory.CreateDirectory(".\\sound");
            }
            this.dic = new SoundDataDirectory(".\\sound");
        }

        public void NextNote()
        {
            this.slideshow.Next();

            var lines = this.slideshow.Note.ToLines();
            string line;
            if (lines.Contains(this.lastPhrase))
            {
                // 改ページしていない
                line = lines.TakeWhile(l => l != this.lastPhrase).Take(2).Last();
            }
            else
            {
                // 改ページ直後
                line = lines.First();
            }

            var stream = this.dic.GetSoundData(line);
            if (stream != null)
            {
                this.Play(stream);
            }
        }

        private void PlayAudio(object sender, GenericEventArgs<Stream> args)
        {
            // For SoundPlayer to be able to play the wav file, it has to be encoded in PCM.
            // Use output audio format AudioOutputFormat.Riff16Khz16BitMonoPcm to do that.
            this.Play(args.EventData);
        }

        private void Play(Stream stream)
        {
            SoundPlayer player = new SoundPlayer(stream);
            player.PlaySync();
            stream.Dispose();
        }
    }
}
