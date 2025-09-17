using UnityEngine;
using UnityEngine.EventSystems;

public class BenchSlot : MonoBehaviour, IDropHandler
{
    // 当一个UI物体在这个槽位上被“放开”时调用
    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerDrag; // 获取被拖拽的物体

        // 如果这个物体是一个“单位图标”
        if (draggedObject.GetComponent<DraggableUnitIcon>() != null)
        {
            // 让图标的父物体变成这个槽位，实现“归位”
            draggedObject.transform.SetParent(this.transform);
            draggedObject.transform.localPosition = Vector3.zero; // 居中
        }
    }
}