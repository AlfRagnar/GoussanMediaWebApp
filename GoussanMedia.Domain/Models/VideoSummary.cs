using System;
using System.Collections;

namespace GoussanMedia.Domain.Models
{
    public class VideoSummary : IEnumerable
    {
        private readonly Videos[] _video;

        public VideoSummary(Videos[] vArray)
        {
            _video = new Videos[vArray.Length];

            for (int i = 0; i < vArray.Length; i++)
            {
                _video[i] = vArray[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public VideoEnum GetEnumerator()
        {
            return new VideoEnum(_video);
        }
    }

    public class VideoEnum : IEnumerator
    {
        private readonly Videos[] _video;

        private int position = -1;

        public VideoEnum(Videos[] list)
        {
            _video = list;
        }

        public bool MoveNext()
        {
            position++;
            return (position < _video.Length);
        }

        public void Reset()
        {
            position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public Videos Current
        {
            get
            {
                try
                {
                    return _video[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}