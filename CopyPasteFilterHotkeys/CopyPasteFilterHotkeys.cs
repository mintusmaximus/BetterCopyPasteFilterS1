using MelonLoader;
using UnityEngine;
using Unity.Collections;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using System.Drawing;





// Conditional compilation example for IL2CPP and MONO
// #if <Build config> is used to check the build configuration
#if IL2CPP
using Il2CppScheduleOne.UI;
using Il2CppScheduleOne.UI.Items;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Interaction;
using Il2CppScheduleOne.Storage;
#elif MONO
using ScheduleOne.UI;
using ScheduleOne.UI.Items;
using ScheduleOne.ItemFramework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.Storage;
#endif

[assembly: MelonInfo(typeof(CopyPasteFilterHotkeys.CopyPasteFilterHotkeys), "CopyPasteFilterHotkeys", "1.0.0", "FemboyDealer", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace CopyPasteFilterHotkeys
{
    public class CopyPasteFilterHotkeys : MelonMod
    {

        //Hotkey Handeling
        private MelonPreferences_Category _category;
        private MelonPreferences_Entry<KeyCode> _copyKey;
        private MelonPreferences_Entry<KeyCode> _pasteKey;
        private SlotFilter _copiedFilter;

        //Display Variables
        private int _maxFrame = 100;
        private int _frameCounter = 0;
        private string _currentMessage = "";
        private Color _currentColor = Color.green;

        //Saved Slots Cache 
        private List<SlotFilter> _filterSlots = new List<SlotFilter>();

        public override void OnInitializeMelon()
        {
            _category = MelonPreferences.CreateCategory("CopyPasteFilterHotkeys", "Copy Paste Filter Hotkeys Settings");
            _copyKey = _category.CreateEntry("CopyKey", KeyCode.LeftBracket, "Copy Filter Hotkey");
            _copyKey.Description = "Key to use to copy filter settings";
            _pasteKey = _category.CreateEntry("PasteKey", KeyCode.RightBracket, "Paste Filter Hotkey");
            _pasteKey.Description = "Key to use to paste filter settings";
            LoggerInstance.Msg($"Initialized. Use <{_copyKey.Value}> to copy a filter, and <{_pasteKey.Value}> to paste a filter");
        }

        public override void OnLateUpdate()
        {
            if (Singleton<ItemUIManager>.InstanceExists) //make sure in game
            {
                void CustomLog(string msg, Color color)
                {
                    _currentMessage = msg;
                    _currentColor = color;
                    _frameCounter = 0;
                }

                if (_frameCounter >= _maxFrame)
                {
                    var CurfewPrompt = HUD.Instance.CurfewPrompt;
                    CurfewPrompt.gameObject.SetActive(false);
                    _frameCounter = -1;
                }
                if (_frameCounter >= 0)
                {
                    var CurfewPrompt = HUD.Instance.CurfewPrompt;
                    CurfewPrompt.text = _currentMessage;
                    CurfewPrompt.color = _currentColor;
                    CurfewPrompt.gameObject.SetActive(true);
                    _frameCounter++;
                }


                if (Input.GetKeyDown(_copyKey.Value)) //Copy Object
                {
                    InteractableObject hovered = InteractionManager.Instance.HoveredInteractableObject;
                    if (hovered != null)
                    {
                        //LoggerInstance.Msg($"Hovered on {hovered.name}");
                        var itemSlotsObj = hovered.GetComponentInParent<IItemSlotOwner>();
                        var slots = itemSlotsObj.ItemSlots;

                        _filterSlots.Clear();
                        foreach (var slot in slots)
                        {
                            //LoggerInstance.Msg($"Slot: {slot.PlayerFilter.Type} | {slot.PlayerFilter.ItemIDs.FirstOrDefault()}... |  {slot.PlayerFilter.AllowedQualities.FirstOrDefault()}...");
                            _filterSlots.Add(slot.PlayerFilter.Clone());
                        }
                        //LoggerInstance.Msg($"{slots.Count()} slots copied!");


#if IL2CPP
                        CustomLog($"Copied {slots.Count} slots.", Color.green);
#elif MONO
                        CustomLog($"Copied {slots.Count()} slots.", Color.green);
#endif
                    }
                }
                if (Input.GetKeyDown(_pasteKey.Value)) //Paste Object
                {
                    InteractableObject hovered = InteractionManager.Instance.HoveredInteractableObject;
                    if (hovered != null &&
                       _filterSlots?.Count() > 1)
                    {
                        var itemSlotsObj = hovered.GetComponentInParent<IItemSlotOwner>();
                        var slots = itemSlotsObj.ItemSlots;


#if IL2CPP
                        if (slots.Count == _filterSlots.Count())
#elif MONO
                        if (slots.Count() == _filterSlots.Count())
#endif
                        {
                            for (int i = 0; i < slots.Count; i++)
                            {
                                if (slots[i].CanPlayerSetFilter)
                                    slots[i].PlayerFilter = _filterSlots[i].Clone();
                            }
                            LoggerInstance.Msg($"Pasted!");
                            CustomLog($"Pasted!", Color.green);
                        }
                        else
                            CustomLog($"Slot count missmatch!", Color.red);
                    }
                    else
                        CustomLog($"Nothing to copy.", Color.white);
                }
            }

            if (Singleton<ItemUIManager>.InstanceExists && Singleton<ItemUIManager>.Instance.HoveredSlot != null)
            {
                if (Input.GetKeyDown(_copyKey.Value)) //Copy Slot
                {
                    if (Singleton<ItemUIManager>.Instance.HoveredSlot.assignedSlot.PlayerFilter != null &&
                        Singleton<ItemUIManager>.Instance.HoveredSlot.assignedSlot.CanPlayerSetFilter)
                    {
                        _copiedFilter = Singleton<ItemUIManager>.Instance.HoveredSlot.assignedSlot.PlayerFilter.Clone();
                        Singleton<ItemUIManager>.Instance.HoveredSlot.UpdateUI();
                    }
                }

                if (Input.GetKeyDown(_pasteKey.Value)) //Paste Slot
                {
                    if (_copiedFilter != null &&
                        Singleton<ItemUIManager>.Instance.HoveredSlot.assignedSlot.CanPlayerSetFilter)
                    {
                        Singleton<ItemUIManager>.Instance.HoveredSlot.assignedSlot.SetPlayerFilter(_copiedFilter.Clone());
                        Singleton<ItemUIManager>.Instance.HoveredSlot.UpdateUI();
                    }
                }
            }
        }
    }
}