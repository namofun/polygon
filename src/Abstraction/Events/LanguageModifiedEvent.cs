using MediatR;
using Polygon.Entities;

namespace Polygon.Events
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
