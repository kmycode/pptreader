using Microsoft.Office.Interop.PowerPoint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PptReader.Models.Office
{
    class PptModel : IDisposable
    {
        private readonly string path = @"C:\Users\KMY\Documents\visual studio 2017\Projects\PptReader\PptReader\test.pptx";
        private Application appClass;
        private Presentation ppt;

        public PptModel()
        {
            this.appClass = new Application();
            this.ppt = this.appClass.Presentations.Open(this.path);
        }

        public IEnumerable<string> GetNotes()
        {
            for (int i = 1; i <= this.ppt.Slides.Count; i++)
            {
                yield return this.ppt.Slides[i].NotesPage.Shapes.Placeholders[2].TextFrame.TextRange.Text;
            }
        }

        public void SaveNotes()
        {
            var notes = this.GetNotes().Distinct().Where(n => !string.IsNullOrEmpty(n));
            File.WriteAllLines("notes.txt", notes);
        }

        public void Dispose()
        {
            this.ppt.Close();
        }

        public SlideShow StartSlideShow()
        {
            var settings = this.ppt.SlideShowSettings;

            settings.StartingSlide = 1;
            settings.EndingSlide = ppt.Slides.Count;

            var slideshow = settings.Run();
            slideshow.Activate();

            return new SlideShow(slideshow);
        }
    }

    class SlideShow
    {
        private SlideShowWindow win;

        public int PageNum
        {
            get
            {
                return this.win.View.CurrentShowPosition;
            }
        }

        public string Note
        {
            get
            {
                return this.win.Presentation.Slides[this.win.View.CurrentShowPosition].NotesPage.Shapes.Placeholders[2].TextFrame.TextRange.Text;
            }
        }

        public SlideShow(SlideShowWindow window)
        {
            this.win = window;
        }

        public void Next()
        {
            this.win.View.Next();
        }

        public void Previous()
        {
            this.win.View.Previous();
        }
    }
}
