﻿using System;
using CodeBase.Data.Settings;

namespace CodeBase.Services.Localization
{
    public class LocalizationService : ILocalizationService
    {
        public Language Language { get; private set; }

        public event Action LanguageChanged;

        public LocalizationService(Language language) => 
            ChangeLanguage(language);

        public void ChangeLanguage(Language language)
        {
            Language = language;
            LanguageChanged?.Invoke();
        }

        public string GetText(string russian, string turkish, string english)
        {
            switch (Language)
            {
                case Language.RU:
                    return russian;
                case Language.TR:
                    return turkish;
                default:
                    return english;
            }
        }
    }
}