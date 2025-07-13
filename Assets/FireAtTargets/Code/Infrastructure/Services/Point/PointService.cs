using System;
using FreedLOW.FireAtTargets.Code.Features.GameSystem;
using FreedLOW.FireAtTargets.Code.Infrastructure.AssetManagement;
using FreedLOW.FireAtTargets.Code.StaticData;
using FreedLOW.FireAtTargets.Code.Target;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Point
{
    public class PointService : IPointService, IInitializable
    {
        private readonly IFireAtTargetsSystem _fireAtTargetsSystem;
        private readonly IAssetProvider _assetProvider;
        
        private TargetData _targetData;

        public int CurrentPoints { get; private set; }
        
        public event Action<int> OnPointsChanged;

        public PointService(IFireAtTargetsSystem fireAtTargetsSystem, IAssetProvider assetProvider)
        {
            _fireAtTargetsSystem = fireAtTargetsSystem;
            _assetProvider = assetProvider;
        }

        public async void Initialize()
        {
            _targetData = await _assetProvider.Load<TargetData>(ShootingGalleryAssets.TARGET_DATA);
        }

        public void AddPoint(TargetShootPointType pointType)
        {
            if (pointType == TargetShootPointType.None)
                throw new Exception("Not valid data!");

            CurrentPoints += _targetData.TargetPoints.Find(p => p.ShootPointType == pointType).GivenPoint;
            _fireAtTargetsSystem.AddPoint(CurrentPoints);
            OnPointsChanged?.Invoke(CurrentPoints);
        }

        public void AddExtraPoints(TargetShootPointType pointType)
        {
            if (pointType == TargetShootPointType.None)
                throw new Exception("Not valid data!");

            CurrentPoints += _targetData.TargetPoints.Find(p => p.ShootPointType == pointType).GivenExtraPoint;
            _fireAtTargetsSystem.AddPoint(CurrentPoints);
            OnPointsChanged?.Invoke(CurrentPoints);
        }

        public void Reset()
        {
            CurrentPoints = 0;
        }
    }
}