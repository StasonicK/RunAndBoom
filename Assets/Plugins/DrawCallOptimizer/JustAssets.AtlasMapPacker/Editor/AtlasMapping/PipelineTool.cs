using System;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    internal static class PipelineTool
    {
        private static ListRequest _request;

        private static Action<bool> _onCheckDone;

        public static void IsPipelineSupported(Action<bool> onCheckDone)
        {
            _onCheckDone = onCheckDone;
            _request = Client.List(true);

            EditorApplication.update += WaitForRequest;
        }

        private static bool IsPipelineSupported()
        {
            string pipeline = null;
            var version = 0;

            const string highDefinitionRP = "render-pipelines.high-definition";
            const string universalRP = "render-pipelines.universal";

            foreach (PackageInfo package in _request.Result)
            {
                if (package.name.Contains(universalRP) || package.name.Contains(highDefinitionRP))
                {
                    pipeline = package.name;
                    version = int.TryParse(package.version.Split('.').First(), out var major) ? major : 0;
                }
            }

            return pipeline == null || pipeline.Contains(universalRP) && version >= 8 || pipeline.Contains(highDefinitionRP) && version >= 8;
        }

        private static void WaitForRequest()
        {
            if (_request.IsCompleted)
                _onCheckDone?.Invoke(IsPipelineSupported());

            EditorApplication.update -= WaitForRequest;
        }
    }
}