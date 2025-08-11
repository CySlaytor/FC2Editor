namespace FC2Editor.Core.Nomad
{
    internal class GizmoHelper
    {
        private Axis m_axisConstraint;
        private Plane m_virtualPlane;
        private Vec3 m_virtualPlanePos;
        private CoordinateSystem m_virtualPlaneBase;
        private CoordinateSystem m_virtualPlaneCoords;

        public void InitVirtualPlane(Vec3 planePos, CoordinateSystem planeBase, Axis axisConstraint)
        {
            m_virtualPlanePos = planePos;
            m_virtualPlaneBase = planeBase;
            m_axisConstraint = axisConstraint;
            CoordinateSystem virtualPlaneCoords = default(CoordinateSystem);

            switch (m_axisConstraint)
            {
                case Axis.X:
                    virtualPlaneCoords.axisX = planeBase.axisX;
                    virtualPlaneCoords.axisY = Vec3.Cross(Camera.FrontVector, planeBase.axisX);
                    virtualPlaneCoords.axisY.Normalize();
                    virtualPlaneCoords.axisZ = Vec3.Cross(planeBase.axisX, virtualPlaneCoords.axisY);
                    virtualPlaneCoords.axisZ.Normalize();
                    break;
                case Axis.Y:
                    virtualPlaneCoords.axisX = planeBase.axisY;
                    virtualPlaneCoords.axisY = Vec3.Cross(Camera.FrontVector, planeBase.axisY);
                    virtualPlaneCoords.axisY.Normalize();
                    virtualPlaneCoords.axisZ = Vec3.Cross(planeBase.axisY, virtualPlaneCoords.axisY);
                    virtualPlaneCoords.axisZ.Normalize();
                    break;
                case Axis.Z:
                    virtualPlaneCoords.axisX = planeBase.axisZ;
                    virtualPlaneCoords.axisY = Vec3.Cross(Camera.FrontVector, planeBase.axisZ);
                    virtualPlaneCoords.axisY.Normalize();
                    virtualPlaneCoords.axisZ = Vec3.Cross(planeBase.axisZ, virtualPlaneCoords.axisY);
                    virtualPlaneCoords.axisZ.Normalize();
                    break;
                case Axis.XY:
                    virtualPlaneCoords.axisX = planeBase.axisX;
                    virtualPlaneCoords.axisY = planeBase.axisY;
                    virtualPlaneCoords.axisZ = planeBase.axisZ;
                    break;
                case Axis.XZ:
                    virtualPlaneCoords.axisX = planeBase.axisX;
                    virtualPlaneCoords.axisY = planeBase.axisZ;
                    virtualPlaneCoords.axisZ = planeBase.axisY;
                    break;
                case Axis.YZ:
                    virtualPlaneCoords.axisX = planeBase.axisY;
                    virtualPlaneCoords.axisY = planeBase.axisZ;
                    virtualPlaneCoords.axisZ = planeBase.axisX;
                    break;
            }
            m_virtualPlane = Plane.FromPointNormal(m_virtualPlanePos, virtualPlaneCoords.axisZ);
            m_virtualPlaneCoords = virtualPlaneCoords;
        }

        public bool GetVirtualPos(out Vec3 pos)
        {
            Editor.GetWorldRayFromScreenPoint(Editor.Viewport.NormalizedMousePos, out Vec3 raySrc, out Vec3 rayDir);
            if (!m_virtualPlane.RayIntersect(raySrc, rayDir, out pos))
            {
                return false;
            }

            switch (m_axisConstraint)
            {
                case Axis.X:
                    pos = Vec3.Dot(pos, m_virtualPlaneBase.axisX) * m_virtualPlaneBase.axisX;
                    break;
                case Axis.Y:
                    pos = Vec3.Dot(pos, m_virtualPlaneBase.axisY) * m_virtualPlaneBase.axisY;
                    break;
                case Axis.Z:
                    pos = Vec3.Dot(pos, m_virtualPlaneBase.axisZ) * m_virtualPlaneBase.axisZ;
                    break;
            }
            return true;
        }
    }
}