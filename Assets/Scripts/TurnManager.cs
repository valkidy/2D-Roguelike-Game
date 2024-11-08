using UnityEngine;

public class TurnManager
{
   private int m_TurnCount;

   public event System.Action OnTick;

   public TurnManager()
   {
       m_TurnCount = 1;
   }

   public void Tick()
   {
       m_TurnCount += 1;
       Debug.Log("Current turn count : " + m_TurnCount);

       if(OnTick != null)
       {
            OnTick.Invoke();
       }
   }
}
