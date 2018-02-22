using System;

namespace Zebble.Plugin.MBox
{
    using Zebble.Services;

    public class Annotation
    {
        private string _id = string.Empty;
        public string Id
        {
            get => _id;
            set
            {
                if (_id == value) return;
                _id = value;
                AnnotationIdChanged.Raise();
                AnnotationChanged.Raise();
            }
        }

        public AsyncEvent AnnotationIdChanged = new AsyncEvent();

        string _title = string.Empty;
        public string Title
        {
            get => _title;
            set
            {
                if (_title == value) return;
                _title = value;
                AnnotationTitleChanged.Raise();
                AnnotationChanged.Raise();
            }
        }
        public AsyncEvent AnnotationTitleChanged = new AsyncEvent();

        private string _subtitle = string.Empty;
        public string SubTitle
        {
            get => _subtitle;
            set
            {
                if (_subtitle == value) return;
                _subtitle = value;
                AnnotationSubtitleChanged.Raise();
                AnnotationChanged.Raise();
            }
        }

        public AsyncEvent AnnotationSubtitleChanged = new AsyncEvent();

        public AsyncEvent AnnotationChanged = new AsyncEvent();
    }
}