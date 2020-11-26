using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UpdateRarity;
/// <summary>
/// создает рецепты в книге, листание рецептов
/// </summary>
public class CraftBookSlot : MonoBehaviour
{
    [SerializeField]
    private CreateCraftSlot[] slots; // слоты в книге крафта (4)
    [SerializeField]
    private TypeOfItem slotType; //текущий тип предметов
    private int currentPage;    // текуща€ страница 
    public static CraftBookSlot craftBook;  //при изучении рецепта в UpdateRarity обновл€ет книгу
    public void UpdateSlots() // обновл€ет информацию о предметах в книге
    {   
        List<CraftingItems> items = openedReciept[(int)slotType]; // получение списка открытых рецептов конкретного типа
        int slot = currentPage*slots.Length; // вычисл€ем позицию первого предмета текущего листа
        for (int i = 0; i < slots.Length; i++) // заполн€ем страницу рецептами, если пусто, то пустое место
        {   if(i+slot >= items.Count) { slots[i].image.sprite = null; slots[i].reciept = null; continue; }
            slots[i].image.sprite = items[i+slot].reciept.itemtoRelease.icon;
            slots[i].reciept = items[i+slot];
        }
    }
    public void Pages(bool forward)  // на кнопке. листает вперед или назад
    {
        int pages = openedReciept[(int)slotType].Count / slots.Count();  // кол-во листов исход€ из кол-ва рецептов.
        currentPage = forward ?                                          //устанавливает номер страницы
        (currentPage = pages == currentPage ? 0 : currentPage += 1) :    //(если вперед)если это последн€€ страница, то  делает текущую 0
        (currentPage = currentPage ==0 ? pages : currentPage -= 1);      //(если назад) если перва€ страница, то устанавливает последнюю страницу 
        UpdateSlots(); //обновление
    }
    private void Awake()
    {
        craftBook = this;
    }
    public void Listing(bool forward)  // листинг рецептов в окне крафта
    {
        List<CraftingItems> items = openedReciept[(int)slotType]; // получение списка открытых рецептов конкретного типа
        CraftingItems craftingItems = FinalCraftingSlot.finalCrafting.reciept; // ролучение текущего рецепта
        int index=items.IndexOf(craftingItems);  // индекс предмета в листе
        FinalCraftingSlot.finalCrafting.Construct(forward    
        ? (items.Count == index + 1 ? items[0] : items[index + 1])  //листание вперед
        : (index==0 ? items[items.Count-1]: items[index - 1]));     //листание назад
    }
}
