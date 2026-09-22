//hodnoty
using console_blockBlast_AI;
using System.Collections.Specialized;
using System.Data;
using System.Numerics;

//static costs //hing code explosion radius

Game game = new Game(new Costs(80.525505f, new float[] { 0.38554317f, 1.3995218f, 12.418425f, 57.876907f, 107.811005f }));
game.PlayWithUI();

Evolution evo = new Evolution();
evo.EvolveRepeatedly(10,10);
