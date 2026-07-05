using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;

public class HydrologyEngine
{
    private readonly IHydrologyPlane plane;
    private readonly HydrologyConfig config;
    private readonly IRainfallProvider rainfallProvider;

    private readonly D8FlowDirectionModel d8FlowDirectionModel;
    private readonly SurfaceFlowModel surfaceFlowModel;
    private readonly BasinFlowRouter basinFlowRouter;
    private readonly LakeStorageModel lakeStorageModel;
    private readonly AquiferRechargePort aquiferRechargePort;

    private readonly FlowDirection[,] flowDirections;

    private readonly int[,] lakeIds;
    private readonly int[,] aquiferIds;
    private readonly int[,] basinIds;
    private readonly int[,] demMask;

    public HydrologyState State { get; }


    public HydrologyEngine(IHydrologyPlane plane, HydrologyConfig config, IRainfallProvider rainfallProvider, int[,] lakeIds = null, int[,] aquiferIds = null, int[,] basinIds = null, int[,] demMask = null)
    {
        this.plane = plane;
        this.config = config;
        this.rainfallProvider = rainfallProvider;

        this.lakeIds = lakeIds;
        this.aquiferIds = aquiferIds;
        this.basinIds = basinIds;
        this.demMask = demMask;

        State = new HydrologyState(
            plane.Rows,
            plane.Columns
        );

        d8FlowDirectionModel = new D8FlowDirectionModel();
        surfaceFlowModel = new SurfaceFlowModel();
        basinFlowRouter = new BasinFlowRouter();
        lakeStorageModel = new LakeStorageModel();
        aquiferRechargePort = new AquiferRechargePort();

        flowDirections = d8FlowDirectionModel.Compute(plane);
    }

    public bool Step()
    {
        if (!IsConfigured())
        {
            return false;
        }

        ClearInactiveCells();

        if (config.EnableRainfall)
        {
            State.AddRainfall(
                rainfallProvider,
                config.TimeStep,
                demMask
            );
        }

        if (config.EnableSurfaceFlow)
        {
            surfaceFlowModel.Update(
                State,
                plane,
                flowDirections,
                config.SurfaceFlowRate,
                config.MinWaterDepthToFlow,
                demMask
            );
        }

        if (config.EnableLakeStorage && lakeIds != null)
        {
            lakeStorageModel.StoreWaterInLakes(
                State,
                lakeIds,
                demMask
            );
        }

        if (config.EnableAquiferRecharge && aquiferIds != null)
        {
            aquiferRechargePort.RechargeAquifers(
                State,
                aquiferIds,
                config.AquiferRechargeRate,
                demMask
            );
        }

        if (config.EnableBasinRouting && basinIds != null)
        {
            basinFlowRouter.Update(
                State,
                basinIds,
                demMask
            );
        }

        ClearInactiveCells();

        State.AdvanceTime(config.TimeStep);

        return true;
    }

    public float GetTotalSurfaceWater()
    {
        return State.GetTotalSurfaceWater();
    }

    public float GetStoredLakeWater(int lakeId)
    {
        return lakeStorageModel.GetStoredWater(lakeId);
    }

    public float GetAquiferRecharge(int aquiferId)
    {
        return aquiferRechargePort.GetRecharge(aquiferId);
    }

    public float GetBasinSurfaceWater(int basinId)
    {

        return basinFlowRouter.GetSurfaceWater(basinId);
    }

    private bool IsConfigured()
    {
        if (config == null) return false;
        if (State == null) return false;
        if (plane == null) return false;

        if (config.EnableRainfall && rainfallProvider == null) return false;

        if (config.EnableSurfaceFlow)
        {
            if (surfaceFlowModel == null) return false;
            if (flowDirections == null) return false;
        }

        if (config.EnableAquiferRecharge && aquiferRechargePort == null) return false;

        if (config.EnableBasinRouting)
        {
            if (basinFlowRouter == null) return false;
            if (basinIds == null) return false;
        }

        return true;
    }

    private bool IsActiveCell(int x, int y)
    {
        if (demMask == null)
            return true;

        return demMask[x, y] != 0;
    }

    private void ClearInactiveCells()
    {
        if (demMask == null)
            return;

        int width = State.SurfaceWater.GetLength(0);
        int height = State.SurfaceWater.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!IsActiveCell(x, y))
                {
                    State.SurfaceWater[x, y] = 0f;
                }
            }
        }
    }
}
