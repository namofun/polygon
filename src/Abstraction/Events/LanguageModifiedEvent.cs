using MediatR;
using Xylab.Polygon.Entities;

namespace Xylab.Polygon.Events
{
    public class LanguageModifiedEvent : INotification
    {
        public Language Language { get; }

        public LanguageModifiedEvent(Language language)
        {
            Language = language;
        }
    }
}
