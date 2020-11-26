using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UpdateRarity;
/// <summary>
/// ������� ������� � �����, �������� ��������
/// </summary>
public class CraftBookSlot : MonoBehaviour
{
    [SerializeField]
    private CreateCraftSlot[] slots; // ����� � ����� ������ (4)
    [SerializeField]
    private TypeOfItem slotType; //������� ��� ���������
    private int currentPage;    // ������� �������� 
    public static CraftBookSlot craftBook;  //��� �������� ������� � UpdateRarity ��������� �����
    public void UpdateSlots() // ��������� ���������� � ��������� � �����
    {   
        List<CraftingItems> items = openedReciept[(int)slotType]; // ��������� ������ �������� �������� ����������� ����
        int slot = currentPage*slots.Length; // ��������� ������� ������� �������� �������� �����
        for (int i = 0; i < slots.Length; i++) // ��������� �������� ���������, ���� �����, �� ������ �����
        {   if(i+slot >= items.Count) { slots[i].image.sprite = null; slots[i].reciept = null; continue; }
            slots[i].image.sprite = items[i+slot].reciept.itemtoRelease.icon;
            slots[i].reciept = items[i+slot];
        }
    }
    public void Pages(bool forward)  // �� ������. ������� ������ ��� �����
    {
        int pages = openedReciept[(int)slotType].Count / slots.Count();  // ���-�� ������ ������ �� ���-�� ��������.
        currentPage = forward ?                                          //������������� ����� ��������
        (currentPage = pages == currentPage ? 0 : currentPage += 1) :    //(���� ������)���� ��� ��������� ��������, ��  ������ ������� 0
        (currentPage = currentPage ==0 ? pages : currentPage -= 1);      //(���� �����) ���� ������ ��������, �� ������������� ��������� �������� 
        UpdateSlots(); //����������
    }
    private void Awake()
    {
        craftBook = this;
    }
    public void Listing(bool forward)  // ������� �������� � ���� ������
    {
        List<CraftingItems> items = openedReciept[(int)slotType]; // ��������� ������ �������� �������� ����������� ����
        CraftingItems craftingItems = FinalCraftingSlot.finalCrafting.reciept; // ��������� �������� �������
        int index=items.IndexOf(craftingItems);  // ������ �������� � �����
        FinalCraftingSlot.finalCrafting.Construct(forward    
        ? (items.Count == index + 1 ? items[0] : items[index + 1])  //�������� ������
        : (index==0 ? items[items.Count-1]: items[index - 1]));     //�������� �����
    }
}
