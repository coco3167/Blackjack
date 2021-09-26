using System;
using System.Collections.Generic;
using System.Linq;

namespace Blackjack
{
	class Program
	{
		static void Main(string[] args) 
		{
			//Choix Nom
			Console.Write("Choose your name, human. ");
			string nom = Console.ReadLine();
			
			//Initialisation cartes de jeu
			Dictionary<string, int> dict = new Dictionary<string, int>()
			{
				{ "1",1},{"2",2},{"3",3},{"4",4},{"5",5},{"6",6},{"7",7},
				{"8",8},{"9",9},{"10",10},{"V",10},{"D",10},{"R",10},
			};
			List<string> joueurH = new List<string>();
			List<string> joueurO = new List<string>();
			List<string> paquet = new List<string>();

			//Choix nombre de paquets
			Console.Write("\nHow many deck do you desire ? ");
			string input = Console.ReadLine();
			int nbPaquet;
			while(!int.TryParse(input, out nbPaquet) || nbPaquet <= 0)
            {
				Console.WriteLine("\nPlease choose a whole number strictly above 0.");
				Console.Write("How many deck do you desire ? ");
				input = Console.ReadLine();
            }

			Console.Write("If you want to play in hard mode, enter h else just press enter. ");
			input = Console.ReadLine();
			bool hard = (input == "h");

			Console.Clear();
			int[] score = new int[2];

			//Génération du paquet de cartes
			foreach(string carte in dict.Keys)
			{
				paquet.AddRange(Enumerable.Repeat(carte, nbPaquet * 4));
			}
			paquet = paquet.OrderBy(x => Guid.NewGuid()).ToList();
			int rangDerniereCarte = paquet.Count()-1;

			//Distribution des cartes
			for(int loop = 0; loop < 2; loop++)
			{
				rangDerniereCarte = DistributionCarte(joueurH, paquet, rangDerniereCarte);
			}
			for(int loop = 0; loop < 2; loop++)
			{
				rangDerniereCarte = DistributionCarte(joueurO, paquet, rangDerniereCarte);
			}

			score = AffichageJeux(joueurH, joueurO, dict, nom);

			bool stopJoueur, stopOrdinateur, finPartie; stopJoueur = stopOrdinateur = finPartie = false;
			while(!finPartie)
			{
				//Tour Joueur---------------------------------------------------------------------
				if(!stopJoueur)
				{
					//Décision Joueur
					Console.WriteLine("Voulez-vous piocher une nouvelle carte ?");
					Console.WriteLine("o - Oui");
					Console.WriteLine("n - Non");
					string choixJoueur = Console.ReadLine();
					if(choixJoueur == "o")
					{
						rangDerniereCarte = DistributionCarte(joueurH, paquet, rangDerniereCarte);
						Console.WriteLine($"\n{nom} : pioche une carte");
					}
					else
					{
						Console.WriteLine($"\n{nom} : s'arrête là.");
						stopJoueur = true;
					}
				}

				//Tour Ordinateur------------------------------------------------------------------
				if (!stopOrdinateur)
				{
					if (hard)
					{
						if(score[0] <= 21 && (score[1] < score[0] || score[1] < 15))
                        {
							rangDerniereCarte = OrdinateurPiocheCarte(rangDerniereCarte, joueurO, paquet);
                        }
                        else
                        {
							stopOrdinateur =  OrdinateurArretCarte();
                        }
					}
					else
                    {
						if (score[1] <= 15 && score[0] <= 21)
						{
							rangDerniereCarte = OrdinateurPiocheCarte(rangDerniereCarte, joueurO, paquet);
						}
						else
						{
							stopOrdinateur = OrdinateurArretCarte();
						}
					}
				}

				Console.Write("\nPress enter to flip the cards\n");
				Console.ReadLine();
				Console.Clear();

				//Affichage des jeux
				score = AffichageJeux(joueurH, joueurO, dict, nom);

				//Fin de partie
				finPartie = (stopOrdinateur && stopJoueur) || (score[1] >= 21 || score[0] >= 21);
			}

			//mettre truc pour bloquer resultats jusqu'à enter
			Console.WriteLine("The game is over, press enter to display the results.");
			Console.ReadLine();
			Console.Clear();

			//Affichage final en fonction du vainqueur/de la victorieuse
			if(score[1] == 21)
			{
				Console.WriteLine("Blackjack de l'ordinateur !");
			}
			else if(score[0] == 21)
			{
				Console.WriteLine($"Blackjack de {nom}");
			}
			else if(score[1] > 21)
			{
				Console.WriteLine($"L'ordinateur a dépassé 21 avec {score[1]} pts");
			}
			else if(score[0] > 21)
			{
				Console.WriteLine($"{nom} a dépassé 21 avec {score[0]} pts");
			}
			else if(score[1] > score[0])
			{
				Console.WriteLine($"Victoire de l'ordinateur avec {score[1]} pts devant {score[0]} pts");
			}
			else if(score[1] < score[0])
			{
				Console.WriteLine($"Victoire de {nom} avec {score[0]} pts devant {score[1]} pts");
			}
			else
			{
				Console.WriteLine($"Egalité de {score[0]} pts");
			}
			Console.ReadLine();
		}

		static int[] AffichageJeux(List<string> joueurH, List<string> joueurO, Dictionary<string,int> dict, string nom)
		{
			//Calcul du score puis affichage des jeux
			int scoreH = Score(dict, joueurH);
			
			string jeuH = joueurH[0] + " " + joueurH[1] + CartesSup(joueurH);
			string jeuO = "? " + joueurO[1] + CartesSup(joueurO);

			Console.WriteLine($"({scoreH} pts) {nom} : {jeuH}");
			Console.WriteLine($"Ordinateur : {jeuO}\n");
			return new int[] { scoreH, Score(dict, joueurO) };
		}
		static string CartesSup(List<string> joueur)
		{
			//Subfonction d'AffichageJeux pour afficher les cartes après la deuxième
			string cartes = "";
			for(int loop = 0; loop < joueur.Count() - 2; loop++)
			{
				cartes += " " + joueur[loop + 2];
			}
			return cartes;
		}
		static int DistributionCarte(List<string> joueur, List<string> paquet, int rangDerniereCarte)
		{
			//On garde l'index de la dernière carte du paquet constamment à jour
			joueur.Add(paquet[rangDerniereCarte]);
			paquet.RemoveAt(rangDerniereCarte);
			return rangDerniereCarte - 1;
		}
		static int Score(Dictionary<string, int> dict, List<string> joueur)
		{
			//Calcul du score en fonction du nombre d'as (il existe sans doute un moyen plus simple
			//Sachant que 2 As ne peuvent valoir 11 tous deux). La valeur de l'As changeant est voulu
			//(interpretation des règles parfois un peu floues)
			int sum = 0;
			int asCount = 0;
			for(int loop = 0; loop < joueur.Count(); loop++)
			{
				if(joueur[loop] != "A")
				{
					sum += dict[joueur[loop]];
				}
				else
				{
					asCount++;
				}
			}
			for(int loop = 0; loop < asCount; loop++)
			{
				if(sum + 11 > 21)
				{
					sum++;
				}
				else
				{
					sum += 11;
				}
			}
			return sum;
		}
		static int OrdinateurPiocheCarte(int rangDerniereCarte, List<string> joueurO, List<string> paquet)
        {
			Console.WriteLine("Ordinateur : pioche une carte.");
			return DistributionCarte(joueurO, paquet, rangDerniereCarte);
		}
		static bool OrdinateurArretCarte()
        {
			Console.WriteLine("Ordinateur : s'arrête là.");
			return true;
        }
	}
}