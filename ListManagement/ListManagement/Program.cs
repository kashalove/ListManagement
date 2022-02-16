using ListManagement.models;
using ListManagement.services;
using Newtonsoft.Json;

namespace ListManagement
{
    public class Program
    {
        static void Main(string[] args)
        {
            // preparations for saving
            var directory = Environment.SpecialFolder.ApplicationData;
            var filename = directory + "savefile.json";

            Console.WriteLine("Welcome to the List Management App");
            var itemService = ItemService.Current;


            int selection = 0;
            while (selection != 10)
            {
                PrintMenu();
                // check if selection is a good int
                if (int.TryParse(Console.ReadLine(), out selection))
                {
                    Item inputItem;
                    if (selection == 1)
                    {
                        //C - Create
                        //ask for property values
                        Console.WriteLine("1. New Calendar Appointment");
                        Console.WriteLine("2. New ToDo");
                        int miniMenu;
                        if (int.TryParse(Console.ReadLine(), out miniMenu))
                        {
                            if (miniMenu != 1 && miniMenu != 2)
                                break;

                            if (miniMenu == 1)
                            {
                                inputItem = new Appointment();
                            }
                            else
                            {
                                inputItem = new ToDo();
                            }
                            FillProperties(inputItem);

                            itemService.Add(inputItem);
                        }

                    }
                    else if (selection == 2)
                    {
                        //D - Delete/Remove

                        try
                        {
                            Console.WriteLine("Select item to delete");
                            if (int.TryParse(Console.ReadLine(), out selection))
                            {
                                itemService.Remove(itemService.Items[selection - 1]);
                            }
                            else
                            {
                                Console.WriteLine("Sorry, I can't find that item!");
                            }
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Error, nothing was deleted");
                        }
                    }
                    else if (selection == 3)
                    {
                        //U - Update/Edit
                        try
                        {

                            Console.WriteLine("Select item to edit?");
                            if (int.TryParse(Console.ReadLine(), out selection))
                            {

                                if (itemService.Items[selection - 1] != null)
                                {
                                    FillProperties(itemService.Items[selection - 1]);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Sorry, I can't find that item!");
                            }
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Error, nothing was edited");
                        }
                    }
                    else if (selection == 4)
                    {
                        //Complete Task
                        try
                        {
                            Console.WriteLine("Select item to complete?");
                            if (int.TryParse(Console.ReadLine(), out selection))
                            {
                                var selectedItem = itemService.Items[selection - 1] as ToDo;

                                if (selectedItem != null)
                                {
                                    selectedItem.IsCompleted = true;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Item not located");
                            }
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Error, nothing was completed");
                        }
                    }
                    else if (selection == 5)
                    {
                        //R - Read / List uncompleted tasks

                        //use LINQ
                        itemService.Items
                            .Where(i => !((i as ToDo)?.IsCompleted ?? true))
                            .ToList()
                            .ForEach(Console.WriteLine);

                    }
                    else if (selection == 6)
                    {
                        //R - Read / List all tasks
                        string pageSelection = "";
                        while (pageSelection != "E")
                        {
                            Console.WriteLine("Press E to return to main menu");
                            try
                            {

                                foreach (var item in itemService.GetPage())
                                {
                                    Console.WriteLine(item);
                                }
                                pageSelection = Console.ReadLine();

                                if (pageSelection == "N")
                                {
                                    itemService.NextPage();
                                }
                                else if (pageSelection == "P")
                                {
                                    itemService.PreviousPage();
                                }
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("Error, returning to main menu");
                                break;
                            }
                        }


                    }
                    else if (selection == 7)
                    {
                        // search
                        Console.WriteLine("Input text to search");
                        var text = Console.ReadLine();
                        if (!string.IsNullOrEmpty(text))
                        {
                            var found = itemService.Items.Where(item => item.Name.Contains(text) || item.Description.Contains(text) || ((item is Appointment) && ((Appointment)item).Attendees.Contains(text)));
                            if (found.Any())
                            {
                                foreach (var result in found)
                                {
                                    Console.WriteLine(result.ToString());
                                }
                            }
                            else
                                Console.WriteLine("Sorry, no results were located");
                        }
                        else break;
                    }
                    else if (selection == 8)
                    {
                        // load
                        if (File.Exists(filename))
                        {
                            JsonSerializerSettings settings = new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.All,
                            };
                            string jsonBlob = File.ReadAllText(filename);
                            itemService = JsonConvert.DeserializeObject<ItemService>(jsonBlob, settings) ?? ItemService.Current;
                            // null coalescence back to current just in case
                        }

                    }
                    else if (selection == 9)
                    {
                        // save 
                        JsonSerializerSettings settings = new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.All,
                        };
                        string jsonBlob = JsonConvert.SerializeObject(itemService, settings);
                        File.WriteAllText(filename, jsonBlob);
                    }
                    else if (selection != 10)
                    {
                        Console.WriteLine("I don't know what you mean");

                    }

                    /*
                    while (!int.TryParse(Console.ReadLine(), out selection))
                    {
                        Console.WriteLine("Sorry, I don't understand.");
                        PrintMenu();
                    }
                    */
                }
                else
                {
                    Console.WriteLine("User did not specify a valid int!");
                }
            }
        }

        public static void PrintMenu()
        {
            Console.WriteLine("1. Add Item");
            Console.WriteLine("2. Delete Item");
            Console.WriteLine("3. Edit Item");
            Console.WriteLine("4. Complete Item");
            Console.WriteLine("5. List Outstanding");
            Console.WriteLine("6. List All");
            Console.WriteLine("7. Search");
            Console.WriteLine("8. Load");
            Console.WriteLine("9. Save");
            Console.WriteLine("10. Exit");
        }

        public static void FillProperties(Item item)
        {
            Console.WriteLine("Insert a Name");
            item.Name = Console.ReadLine();
            Console.WriteLine("Insert a Description");
            item.Description = Console.ReadLine()?.Trim();
            if (item is Appointment)
            {
                // default initialization for attendee processing
                string input = "-1";
                Console.Write("When does the appointment start?: ");
                try
                {
                    //type casted to base class to access start
                    ((Appointment)item).Start = DateTime.Parse(Console.ReadLine().Trim());
                }
                catch (Exception)
                {
                    ((Appointment)item).Start = DateTime.Today;
                }
                Console.Write("When does the appointment end?: ");
                try
                {
                    ((Appointment)item).End = DateTime.Parse(Console.ReadLine().Trim());
                }
                catch (Exception)
                {
                    ((Appointment)item).End = DateTime.Today;
                }
                Console.Write("Who are the attendees? Enter a blank to move on: ");
                try
                {
                    while (input != "")
                    {
                        // null coalescence to empty string if null
                        input = Console.ReadLine()?.Trim() ?? "";
                        if (input != "")
                            AddString(((Appointment)item).Attendees, input);
                    }
                }
                catch (Exception)
                {
                    AddString(((Appointment)item).Attendees, "");
                }

            }
            else if (item is ToDo)
            {
                Console.WriteLine("When is the due date?: ");
                try
                {
                    ((ToDo)item).Deadline = DateTime.Parse(Console.ReadLine().Trim());
                }
                catch (Exception)
                {
                    ((ToDo)item).Deadline = DateTime.Today;
                }
            }
        }

        public static void AddString(List<string> strList, string str)
        {
            strList.Add(str);
        }
    }
}