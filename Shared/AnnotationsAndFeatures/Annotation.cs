namespace Zebble.Plugin.MBox
{
    public partial class Annotation
    {
        string id = string.Empty;
        public string Id
        {
            get => id;
            set
            {
                if (id == value) return;
                id = value;
                AnnotationIdChanged.Raise();
                AnnotationChanged.Raise();
            }
        }

        public AsyncEvent AnnotationIdChanged = new AsyncEvent();

        string title = string.Empty;
        public string Title
        {
            get => title;
            set
            {
                if (title == value) return;
                title = value;
                AnnotationTitleChanged.Raise();
                AnnotationChanged.Raise();
            }
        }
        public AsyncEvent AnnotationTitleChanged = new AsyncEvent();

        string subtitle = string.Empty;
        public string SubTitle
        {
            get => subtitle;
            set
            {
                if (subtitle == value) return;
                subtitle = value;
                AnnotationSubtitleChanged.Raise();
                AnnotationChanged.Raise();
            }
        }

        public AsyncEvent AnnotationSubtitleChanged = new AsyncEvent();

        public AsyncEvent AnnotationChanged = new AsyncEvent();
    }
}