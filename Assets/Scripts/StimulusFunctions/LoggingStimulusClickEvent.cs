using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GaborFunctions
{
    public class LoggingStimulusClickEvent : VisualStimulusClickEvent
    {
        protected override void OnClickPreCallback(PointerEventData eventData)
        {
            LogManager.LogClick(
                id: Target.Id,
                type: Target.ObjectType,
                x: eventData.pressPosition.x,
                y: eventData.pressPosition.y,
                validClick: true);
        }
    }
}