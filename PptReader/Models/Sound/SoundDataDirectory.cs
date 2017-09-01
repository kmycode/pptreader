using PptReader.Models.Cloud.Speech;
using PptReader.Models.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PptReader.Models.Sound
{
    class SoundDataDirectory
    {
        private readonly List<SoundDataIndexItem> index = new List<SoundDataIndexItem>();
        private int maxId;
        private bool isUpdateAbortRequested;

        public string Path { get; }
        public bool IsUpdating { get; private set; }

        public SoundDataDirectory(string dirPath)
        {
            this.Path = dirPath.EndsWith("\\") ? dirPath : dirPath + "\\";

            this.Load();
            this.CleanNoIndexedRaws();
        }

        /// <summary>
        /// インデクスをロードする
        /// </summary>
        private void Load()
        {
            if (File.Exists(this.Path + "index.txt"))
            {
                this.LoadIndex();
            }
        }

        /// <summary>
        /// インデクスをファイルからロードする
        /// </summary>
        private void LoadIndex()
        {
            var indexRaw = File.ReadAllLines(this.Path + "index.txt");
            this.index.Clear();

            foreach (var line in indexRaw)
            {
                var data = line.Split(',');
                if (data.Length >= 2 && int.TryParse(data[0], out int id))
                {
                    if (File.Exists(this.Path + id + ".raw"))
                    {
                        this.index.Add(new SoundDataIndexItem
                        {
                            Text = data[1],
                            Id = id,
                        });
                    }
                }
            }

            if (this.index.Any())
            {
                this.maxId = this.index.Max(i => i.Id);
            }
        }

        /// <summary>
        /// インデクスされていない音声ファイルを削除する
        /// </summary>
        private void CleanNoIndexedRaws()
        {
            var raws = Directory.GetFiles(this.Path, "*.raw");
            var noIndexedRaws = raws.Except(this.index.Select(i => i.Id.ToString() + ".raw"));

            foreach (var fileName in noIndexedRaws)
            {
                File.Delete(this.Path + fileName);
            }
        }

        /// <summary>
        /// ノートを読んで、音声ファイルを保存する
        /// </summary>
        /// <param name="notes"></param>
        public void UpdateIndex(IEnumerable<string> notes)
        {
            while (this.IsUpdating) { }
            this.IsUpdating = true;
            this.isUpdateAbortRequested = false;

            var lines = notes.SelectMany(n => n.Replace("\r", string.Empty).Split('\n')
                             .Where(l => !string.IsNullOrEmpty(l)))
                             .Except(this.index.Select(i => i.Text))
                             .Distinct();
            var soundTasks = new List<SoundDataRequestedEventArgs>();

            // 存在しない音声のファイルをクラウド経由で取得
            foreach (var line in lines)
            {
                var ev = new SoundDataRequestedEventArgs
                {
                    Text = line,
                };
                this.SoundDataRequested?.Invoke(this, ev);
                soundTasks.Add(ev);
            }

            // 取得が終わったものから順次データに追加
            Task.Run(() =>
            {
                while (soundTasks.Any() && !this.isUpdateAbortRequested)
                {
                    foreach (var task in soundTasks.Where(t => t.IsSucceed || t.IsError).ToArray())
                    {
                        soundTasks.Remove(task);

                        if (task.IsSucceed)
                        {
                            this.index.Add(new SoundDataIndexItem
                            {
                                Text = task.Text,
                                Id = this.maxId + 1,
                            });
                            this.maxId++;
                        }
                    }

                    Task.Delay(100).Wait();
                }

                this.IsUpdating = false;
            });
        }

        public void AbortUpdateIndex()
        {
            if (this.IsUpdating)
            {
                this.isUpdateAbortRequested = true;
            }
        }

        /// <summary>
        /// 指定されたテキストの音声データを要求
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Stream GetSoundData(string text)
        {
            var id = this.index.Where(i => i.Text == text);

            if (File.Exists(this.Path + id + ".raw"))
            {
                return File.OpenRead(this.Path + id + ".raw");
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 音声データがクラウドから供給された時に呼び出し
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnAudioAvailabled(object sender, AudioAvailabledEventArgs e)
        {

        }

        /// <summary>
        /// 音声データが要求された時に発行されるイベント
        /// </summary>
        public event EventHandler<SoundDataRequestedEventArgs> SoundDataRequested;

        private class SoundDataIndexItem
        {
            public string Text { get; set; }
            public int Id { get; set; }
        }
    }

    class SoundDataRequestedEventArgs : EventArgs
    {
        public string Text { get; set; }
        public float[] SoundData { get; private set; }
        public bool IsSucceed { get; private set; }
        public bool IsError { get; private set; }
        public void OnSucceed(float[] raw)
        {
            this.SoundData = raw;
            this.IsSucceed = true;
        }
        public void OnError()
        {
            this.IsError = true;
        }
    }
}
