using UnityEngine;
using UnityEngine.UI;

namespace Code.Core.UI
{
    /// <summary>
    /// UI상에 선 그려주는데 쓰는 컴포넌트.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class UILineRenderer : MaskableGraphic
    {
        public Vector2[] points = new Vector2[2];
        public float thickness = 1f;
        public bool center = true;
        public Color lineColor = Color.white;

        public void SetLinePositions(Vector2 point1, Vector2 point2)
        {
            points[0] = point1;
            points[1] = point2;
            SetVerticesDirty();
        }
        
        public void SetLinePositions(Vector2[] linePoints)
        {
            points = linePoints;
            SetVerticesDirty();
        }
        
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (points.Length < 2) return;

            for (int i = 0; i < points.Length - 1; i++) //length-1까지만 간다.
            {
                CreateLineSegment(points[i], points[i+1], vh);
                int index = i * 5;

                vh.AddTriangle(index, index + 1, index + 3);  //0, 1, 3, 
                vh.AddTriangle(index + 3, index + 2, index);  // 3, 2, 0

                if (i != 0)
                {
                    vh.AddTriangle(index, index - 1, index - 3);
                    vh.AddTriangle(index + 1, index - 1, index - 2);
                }
            }
        }
        private void CreateLineSegment(Vector3 point1, Vector3 point2, VertexHelper vh)
        {
            Vector3 offset = center ? (rectTransform.sizeDelta * 0.5f) : Vector2.zero;
            UIVertex vertex = UIVertex.simpleVert; //기본값으로 채워진 구조체가 복사되서 나온다.
            vertex.color = lineColor; 
            
            Quaternion point1Rot = Quaternion.Euler(0f, 0f, RotateToPointToward(point1, point2) + 90f);
            vertex.position = point1Rot * new Vector3(-thickness * 0.5f, 0f); //선분의 왼쪽점
            vertex.position += point1 - offset; //중심점이였다면 오프셋 이동
            vh.AddVert(vertex);
            
            vertex.position = point1Rot * new Vector3(thickness * 0.5f, 0f); //선분의 오른쪽점
            vertex.position += point1 - offset; //중심점이였다면 오프셋 이동
            vh.AddVert(vertex);
            
            //두번째 세그먼트는 아래를 보는 상태에서 회전이라 이미 -90도 되어 있음.
            Quaternion point2Rot = Quaternion.Euler(0, 0, RotateToPointToward(point2, point1) - 90f);
            vertex.position = point2Rot * new Vector3(-thickness * 0.5f, 0f); //선분의 왼쪽점
            vertex.position += point2 - offset; //중심점이였다면 오프셋 이동
            vh.AddVert(vertex);
            
            vertex.position = point2Rot * new Vector3(thickness * 0.5f, 0f); //선분의 오른쪽점
            vertex.position += point2 - offset; //중심점이였다면 오프셋 이동
            vh.AddVert(vertex);
            
            //마지막 포인트를 넣어서 대각선을 그릴 수 있게 해준다.
            vertex.position = point2 - offset; //여긴 선분의 중심점이 된다.
            vh.AddVert(vertex);
        }
        
        private float RotateToPointToward(Vector2 vertex, Vector2 target)
            => Mathf.Atan2(target.y - vertex.y, target.x - vertex.x) * Mathf.Rad2Deg;
    }
}