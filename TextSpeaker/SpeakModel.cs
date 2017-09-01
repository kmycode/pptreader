using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TextSpeaker
{
    class SpeakModel : INotifyPropertyChanged
    {
        private Synthesize cortana = new Synthesize();

        public string Text
        {
            get => this._text;
            set
            {
                if (this._text != value)
                {
                    this._text = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _text = "こんにちは、元気ですか？";

        public SpeakModel()
        {
            //cortana.OnAudioAvailable += PlayAudio;
            this.cortana.OnError += this.ErrorHandler;

            if (!Directory.Exists("sounds"))
            {
                Directory.CreateDirectory("sounds");
            }
        }

        public void Convert()
        {
            Task.Run(async () => await this.ConvertAsync());
        }

        private async Task ConvertAsync()
        {
            string accessToken;

            string requestUri = "https://speech.platform.bing.com/synthesize";

            var lines = this.Text.Replace("\r", "").Split('\n').Where(l => !string.IsNullOrEmpty(l)).Distinct().Where(l => !File.Exists("sounds\\" + l + ".wav"));
            if (lines.Count() <= 0)
            {
                this.Message("変換は行いません");
                return;
            }
            if (lines.Count() > 20)
            {
                this.Message("一度に変換できるのは20語までです。\n現在：" + lines.Count() + "\n最初の20語のみ変換を行います。");
                lines = lines.Take(20);
            }

            var auth = new Authentication("Your API Key");
            try
            {
                accessToken = auth.GetAccessToken();
            }
            catch
            {
                this.Message("認証に失敗しました");
                return;
            }
            this.Message("変換を開始します");

            foreach (var line in lines)
            {
                // Reuse Synthesize object to minimize latency
                await this.cortana.Speak(CancellationToken.None, new Synthesize.InputOptions()
                {
                    RequestUri = new Uri(requestUri),
                    Text = line,
                    VoiceType = Gender.Male,
                    Locale = "ja-JP",
                    VoiceName = "Microsoft Server Speech Text to Speech Voice (ja-JP, Ichiro, Apollo)",
                    OutputFormat = AudioOutputFormat.Riff16Khz16BitMonoPcm,
                    AuthorizationToken = "Bearer " + accessToken,
                }, (binary) =>
                {
                    // callback
                    using (var pcm = new MemoryStream(binary))
                    {
                        using (var writer = new WaveFileWriter("sounds\\" + line + ".wav", new WaveFormat(16000, 16, 1)))
                        {
                            writer.Write(binary, 0, binary.Length);
                        }
                    }
                });
            }

            this.Message("変換完了しました");
        }

        private void Message(string mes)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                System.Windows.MessageBox.Show(mes);
            });
        }

        private void ErrorHandler(object sender, GenericEventArgs<Exception> e)
        {
            this.Message(e.EventData.Message + "\n\n" + e.EventData.StackTrace);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
