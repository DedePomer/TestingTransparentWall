using UnityEngine;


namespace Scripts.Camera
{
    [DisallowMultipleComponent, RequireComponent(typeof(CameraInput))]
    public class CameraConroller : MonoBehaviour
    {
        private static readonly int HolePosition = Shader.PropertyToID("_HolePosition");
        private static readonly int HoleRadius = Shader.PropertyToID("_HoleRadius");

        [Header("Objects")]
        [SerializeField] private Transform homeTransform;

        [Header("CameraOption")]
        [SerializeField] private float cameraSpeed;

        [Header("ShaderOption")]
        [SerializeField] private float sphereRadius = 0.3f;
        [SerializeField] private float maxDistance = 5f;
        [SerializeField] private float holeRadius = 0.5f;
        [SerializeField] private float fadeSpeed = 3f;
        [SerializeField] private LayerMask wallMask;



        private CameraInput _cameraInput;
        private Renderer _lastRenderer;
        private float _currentRadius = 0f;
        private bool _isNear = false;

        private void Awake()
        {
            _cameraInput = GetComponent<CameraInput>();
        }

        private void Update()
        {
            HoleRender();
            Movment();
        }

        private void HoleRender()
        {
            RaycastHit hit;
            bool didHit = Physics.SphereCast(
                    gameObject.transform.position,
                    sphereRadius,
                    gameObject.transform.forward,
                    out hit,
                    maxDistance,
                    wallMask);


            float targetRadius = didHit ? holeRadius : 0f;
            _currentRadius = Mathf.Lerp(_currentRadius, targetRadius, Time.deltaTime * fadeSpeed);


            if (didHit)
            {
                Renderer rend = hit.collider.GetComponent<Renderer>();
                if (rend != null)
                {

                    //if (_lastRenderer != null && _lastRenderer != rend)
                    //    ClearHole(_lastRenderer);

                    _lastRenderer = rend;

                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    rend.GetPropertyBlock(block);
                    block.SetVector(HolePosition, hit.point);
                    block.SetFloat(HoleRadius, _currentRadius);
                    rend.SetPropertyBlock(block);
                }
            }
            else
            {
                if (_lastRenderer != null)
                {
                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    _lastRenderer.GetPropertyBlock(block);
                    block.SetFloat(HoleRadius, _currentRadius);
                    _lastRenderer.SetPropertyBlock(block);

                    if (_currentRadius < 0.01f)
                    {
                        ClearHole(_lastRenderer);
                        _lastRenderer = null;
                    }
                }
            }
        }

        private void ClearHole(Renderer rend)
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            rend.GetPropertyBlock(block);
            block.SetFloat(HoleRadius, 0f);
            rend.SetPropertyBlock(block);
        }

        private void Movment()
        {
            Vector2 directionVectros = _cameraInput.MoveVector;


            Vector3 move =
                (transform.right * directionVectros.x +
                 transform.forward * directionVectros.y);

            transform.position += move * cameraSpeed * Time.deltaTime;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            Vector3 start = transform.position;
            Vector3 end = start + transform.forward * maxDistance;

            
            Gizmos.DrawWireSphere(start, sphereRadius);

           
            Gizmos.DrawWireSphere(end, sphereRadius);

            
            Gizmos.DrawLine(start, end);
        }
    }
}

