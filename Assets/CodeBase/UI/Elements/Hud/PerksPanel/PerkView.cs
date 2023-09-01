using CodeBase.Data.Perks;
using CodeBase.StaticData.Items;

namespace CodeBase.UI.Elements.Hud.PerksPanel
{
    public class PerkView : LevelingItemView
    {
        private PerkItemData _perkItemData;
        private PerkStaticData _perkStaticData;

        protected override void OnEnabled()
        {
            if (ItemData != null)
                ItemData.LevelChanged += ChangeLevel;
        }

        protected override void OnDisabled()
        {
            if (ItemData != null)
                ItemData.LevelChanged -= ChangeLevel;
        }

        public void Construct(PerkItemData perkItemData)
        {
            base.Construct(perkItemData);
            _perkItemData = perkItemData;
            ItemData.LevelChanged += ChangeLevel;
            ChangeLevel();
        }

        private new void ChangeLevel()
        {
            _perkStaticData = StaticDataService.ForPerk(_perkItemData.PerkTypeId, _perkItemData.LevelTypeId);
            LevelingStaticData = _perkStaticData;

            MainTypeImage.sprite = _perkStaticData.MainImage;
            LevelTypeImage.sprite = _perkStaticData.ILevel;

            base.ChangeLevel();
        }
    }
}