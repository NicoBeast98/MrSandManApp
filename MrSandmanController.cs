using Sandman.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sandman.Controllers.Sueño
{
    public class MrSandmanController : Controller
    {
        int AstronautSession = 7;

        // GET: Sueño
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult Main(float? Pulse, int FoodID = 0)
        {
            using (var db = new SandmanEntities())
            {
                List<string> Return = new List<string>();

                if (FoodID != 0) CalIn(FoodID);

                if (Pulse != null)
                {
                    if (IsSleep(Pulse))
                    {
                        //ViewBag.Sleep = "Sleeping...";
                        Return.Add("Sleeping...");
                    }
                    else
                    {
                        SleepTime Sleep = db.SleepTime.Where(x => x.AstronautID == AstronautSession).OrderByDescending(x => x.SleepUp).First();

                        //ViewBag.Sleep = "Awake, slept for the last time: " + Convert.ToString(Sleep.SleepUp - Sleep.SleepDown) + " hours.";
                        Return.Add("Awake, slept for the last time: " + Convert.ToString(Sleep.SleepUp - Sleep.SleepDown) + " hours.");
                    }

                    if (IsEx(Pulse))
                    {
                        CurrentCalories Sleep = db.CurrentCalories.Where(x => x.AstronautID == AstronautSession).First();

                        //ViewBag.Excers = "Exercising, today's calories: " + db.CurrentCalories.Find(AstronautSession).Calories + ".";
                        Return.Add("Exercising, today's calories: " + db.CurrentCalories.Find(AstronautSession).Calories + ".");
                    }
                    else
                    {
                        //ViewBag.Excers = "At rest, today's calories: " + db.CurrentCalories.Find(AstronautSession).Calories + ".";
                        Return.Add("At rest, today's calories: " + db.CurrentCalories.Find(AstronautSession).Calories + ".");
                    }
                }
                return Json(Return, JsonRequestBehavior.AllowGet);
            }
        }

        public bool IsEx(float? puls)
        {
            using (var db = new SandmanEntities())
            {
                Astronauts Astronaut = db.Astronauts.Find(AstronautSession);
                CurrentCalories Calorie = db.CurrentCalories.Find(AstronautSession);

                if (puls >= (Astronaut.Pulse + Astronaut.Pulse * 0.7))
                {
                    Calorie.Calories = Calorie.Calories - CalEx("C");
                    db.SaveChanges();

                    return true;
                }

                if (puls > (Astronaut.Pulse + Astronaut.Pulse * 0.5) && puls < (Astronaut.Pulse + Astronaut.Pulse * 0.7))
                {
                    Calorie.Calories = Calorie.Calories - CalEx("F");
                    db.SaveChanges();

                    return true;
                }

                return false;
            }
        }

        public double? CalEx(string Type)
        {
            using (var db = new SandmanEntities())
            {
                if (Type == "F")
                {

                    return 8 * 0.0157 * db.Astronauts.Find(AstronautSession).Weight;

                }

                if (Type == "C")
                {
                    return 16 * 0.0157 * db.Astronauts.Find(AstronautSession).Weight;
                }
            }
            return 0;
        }

        public bool CalIn(int FoodID)
        {
            try
            {
                using (var db = new SandmanEntities())
                {
                    AstronautFood Cal = new AstronautFood();
                    Cal.AstronautID = AstronautSession;
                    Cal.FoodID = FoodID;

                    db.AstronautFood.Add(Cal);
                    db.SaveChanges();

                    CurrentCalories AstronautCalories = db.CurrentCalories.Find(AstronautSession);
                    Food oFood = db.Food.Find(FoodID);

                    AstronautCalories.Calories += oFood.Calories;
                    db.SaveChanges();

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool IsSleep(float? puls)
        {
            using (var db = new SandmanEntities())
            {
                Astronauts Astronaut = db.Astronauts.Find(AstronautSession);

                if (puls <= (Astronaut.Pulse - Astronaut.Pulse * 0.08))
                {
                    SleepTime Sleep = db.SleepTime.Where(x => x.AstronautID == AstronautSession).OrderByDescending(x => x.SleepDown).First();

                    if (Sleep != null && Sleep.DayComplete == true)
                    {
                        SleepTime ST = new SleepTime();
                        ST.AstronautID = AstronautSession;
                        ST.SleepDown = DateTime.Now;
                        ST.DayComplete = false;

                        db.SleepTime.Add(ST);
                        db.SaveChanges();

                        return true;
                    }

                    return true;
                }
                else
                {
                    var lista = (from a in db.SleepTime
                                 where a.AstronautID == AstronautSession && a.DayComplete == false
                                 select new
                                 {
                                     a.SleepTimeID
                                 }).FirstOrDefault();

                    if (lista != null)
                    {
                        SleepTime act = db.SleepTime.Find(lista.SleepTimeID);
                        act.SleepUp = DateTime.Now;
                        act.DayComplete = true;

                        db.SaveChanges();
                        return false;
                    }
                }
                return false;
            }
        }
    }
}