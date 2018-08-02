using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SimpleInputNamespace
{
	public class AxisInputUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public SimpleInput.AxisInput axis = new SimpleInput.AxisInput();
		public float value = 1f;

		private void Awake()
		{
			Graphic graphic = GetComponent<Graphic>();
			if( graphic != null )
				graphic.raycastTarget = true;
		}

		private void OnEnable()
		{
			axis.StartTracking();
		}

		private void OnDisable()
		{
			axis.StopTracking();
		}

        void SetVal()
        {
            axis.value = value;
        }
        void ReleaseVal()
        {
            axis.value = 0f;
        }
        
        public void OnPointerDown( PointerEventData eventData )
		{
			axis.value = value;
		}

        public void OnPointerEnter(PointerEventData eventData)
        {
            axis.value = value;
        }

        public void OnPointerUp( PointerEventData eventData )
		{
			axis.value = 0f;
		}

        public void OnPointerExit(PointerEventData eventData)
        {
            axis.value = 0f;
        }
    }
}