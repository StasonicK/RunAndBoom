using CodeBase.Data;
using CodeBase.Services;
using CodeBase.Services.StaticData;
using CodeBase.StaticData.Items;
using CodeBase.UI.Services;
using NTC.Global.Cache;
using UnityEngine.UI;

namespace CodeBase.UI.Elements.Hud
{
    public abstract class LevelingItemView : MonoCache
    {
        public Image MainTypeImage;
        public Image LevelTypeImage;

        protected IStaticDataService StaticDataService;
        protected LevelingItemData ItemData;
        protected ILeveling LevelingStaticData;

        private void Awake()
        {
            if (StaticDataService == null)
                StaticDataService = AllServices.Container.Single<IStaticDataService>();
        }

        protected void Construct(LevelingItemData itemData)
        {
            ItemData = itemData;
            ItemData.LevelChanged += ChangeLevel;

            ChangeLevel();
        }

        protected void ChangeLevel()
        {
            if (LevelingStaticData == null || LevelingStaticData.ILevelTypeId == LevelTypeId.None)
            {
                MainTypeImage.ChangeImageAlpha(Constants.Invisible);
                LevelTypeImage.ChangeImageAlpha(Constants.Invisible);
                return;
            }

            if (LevelingStaticData.ILevelTypeId == LevelTypeId.Level_1)
                LevelTypeImage.ChangeImageAlpha(Constants.Invisible);
            else
                LevelTypeImage.ChangeImageAlpha(Constants.Visible);

            MainTypeImage.ChangeImageAlpha(Constants.Visible);
        }
    }
}