//hodnoty
using console_blockBlast_AI;
using System.Collections.Specialized;
using System.Data;
using System.Numerics;

//better implemented costs
//no Gpu

Game game = new Game();
game.PlayWithUI();

Evolution evo = new Evolution();
evo.EvolveRepeatedly(10,10);
