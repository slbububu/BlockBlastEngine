//hodnoty
using BlockBlast;
using BlockBlast4;
using console_blockBlast_AI;
using System.Collections.Specialized;
using System.Data;
using System.Numerics;

//pripraveno pro gpu, last backup pred poslednim krokem

/*
while(true)
{
    GpuGame cgame = new GpuGame(S.rand.Next(), new GpuCosts()); 
    int mc = cgame.PlayAndReturnMoveCount();
    Console.WriteLine(mc);

    Game game = new Game(); 
    mc = game.PlayAndReturnMoveCount();
    Console.WriteLine(mc);

    Console.ReadLine();
}*/

/*
Game game = new Game();
game.PlayWithUI();
*/

/*
for (int i = 0; i < 10; i++)
{
    S.SetSeed(i);
    Console.WriteLine(S.NextRandom());
}
*/

Evolution evo = new Evolution();
evo.EvolveRepeatedly(10, 10);
