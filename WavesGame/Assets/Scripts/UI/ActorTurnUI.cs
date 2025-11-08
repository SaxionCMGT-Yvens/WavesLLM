using System;
using Actors;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ActorTurnUI : MonoBehaviour, IComparable<ActorTurnUI>
    {
        [SerializeField] private Image holderImage;
        [SerializeField] private Image actorImage;
        [SerializeField, ReadOnly] private NavalShip navalShip;
        [SerializeField, ReadOnly] private NavalShipSo navalShipSo;

        [Header("References")]
        [SerializeField] private Sprite availableActorHolderSprite;
        [SerializeField] private Sprite unavailableActorHolderSprite;
        
        public void Initialize(NavalShip newNavalShip)
        {
            navalShip = newNavalShip;
            navalShipSo = newNavalShip.ShipData;
            actorImage.sprite = navalShipSo.shipSprite;
            ToggleAvailability(false);
        }

        public void ToggleAvailability(bool available)
        {
            holderImage.sprite = available ? availableActorHolderSprite : unavailableActorHolderSprite;
        }

        public int CompareTo(ActorTurnUI other)
        {
            return navalShipSo.stats.Speed.CompareTo(other.navalShipSo.stats.Speed);
        }
        
        public NavalShip NavalShip => navalShip;
    }
}