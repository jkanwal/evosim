# evosim

Navigate to `Assets/Scripts/` to find the critical scripts that govern creature behaviour and simulation parameters.

`RunSim.cs` is a script attached to the scene as a whole. It sets the main parameters of the simulation run, such as how many creatures populate each patch, how many patches there are in total, and how often to reset and start a new generation. It also contains the instructions for how to create a new generation, which can be done in 4 different ways -- this is set by toggling the boolean values `global` and `rHigh`.

`CreatureBehaviour.cs` and `Genome.cs` are both attached to each individual creature. `Genome.cs` can be thought of as the "genotype" of the creature, i.e. the instructions for which of its legs are inert, stingers, or grabbers.

`CreatureBehaviour.cs` can be thought of as the "phenotype". It reads off the genome and generates the resultant traits (e.g. leg colour) and behaviour of the creature, such as instructions for how to move, and how to detect and grab or sting objects with each of its limbs, depending on their type.

