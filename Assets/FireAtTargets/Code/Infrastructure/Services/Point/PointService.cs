using System;
using System.Collections.Generic;
using FreedLOW.FireAtTargets.Code.Features.GameSystem;
using FreedLOW.FireAtTargets.Code.Infrastructure.AssetManagement;
using FreedLOW.FireAtTargets.Code.StaticData;
using FreedLOW.FireAtTargets.Code.Target;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Point
{
    /// <summary>
    /// Converts target hits into points. The score itself is owned by <see cref="IFireAtTargetsSystem"/>.
    /// </summary>
    public class PointService : IPointService, IInitializable
    {
        private readonly IFireAtTargetsSystem _fireAtTargetsSystem;
        private readonly IAssetProvider _assetProvider;

        private readonly Dictionary<TargetShootPointType, TargetPointData> _pointsByType = new();

        public PointService(IFireAtTargetsSystem fireAtTargetsSystem, IAssetProvider assetProvider)
        {
            _fireAtTargetsSystem = fireAtTargetsSystem;
            _assetProvider = assetProvider;
        }

        public async void Initialize()
        {
            try
            {
                var targetData = await _assetProvider.Load<TargetData>(ShootingGalleryAssets.TARGET_DATA);
                foreach (var pointData in targetData.TargetPoints)
                    _pointsByType[pointData.ShootPointType] = pointData;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        public void AddPoint(TargetShootPointType pointType)
        {
            if (TryGetPointData(pointType, out var pointData))
                _fireAtTargetsSystem.AddPoint(pointData.GivenPoint);
        }

        public void AddExtraPoints(TargetShootPointType pointType)
        {
            if (TryGetPointData(pointType, out var pointData))
                _fireAtTargetsSystem.AddPoint(pointData.GivenExtraPoint);
        }

        private bool TryGetPointData(TargetShootPointType pointType, out TargetPointData pointData)
        {
            if (pointType == TargetShootPointType.None)
                throw new ArgumentException("Target shoot point type is not set!", nameof(pointType));

            if (_pointsByType.TryGetValue(pointType, out pointData))
                return true;

            Debug.LogError($"[PointService] No point data for {pointType}. Is TargetData loaded and complete?");
            return false;
        }
    }
}
