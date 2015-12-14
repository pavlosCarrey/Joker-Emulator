using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace JokerEmulator
{
    /// <summary>
    /// Logic of Joker Emulator main
    /// </summary>
    public partial class MainWindow : Window
    {

        private BackgroundWorker bw; //ο background worker μας που θα κάνει τη δουλειά.

        public MainWindow()
        {
            InitializeComponent();
        }

        //αυτή η method καλείται από τον worker για να καταγράψει progress.
        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progress.Value = e.ProgressPercentage; //ενημέρωση του progress bar.
            if (e.UserState != null) //Το e.UserState περιέχει ένα αντικείμενο με τα στοιχεία που θα ενημερώσουμε το dataGrid. Αν αυτό υπάρχει
            {
                dataResults.Items.Add(e.UserState); //ενημέρωσε το dataGrid
                dataResults.ScrollIntoView(e.UserState); //σκρόλλαρε στο καινούργιο αντικείμενο.
            }
            else //αν δεν υπάρχει το αντικείμενο
            {
                dataResults.Items.Clear(); //σημαίνει ότι ξεκινάμε καινούργια προσομοίωση, οπότε καθάρισε το dataGrid.
            }

        }

        public void KillWorker() //καθάρισε τον worker.
        {
            bw.CancelAsync(); //σταμάτησε τον worker. Ενεργοποιείται το event handler της ολοκλήρωσης της δουλειάς του worker
            bw.Dispose(); //αφαίρεσε τα resources του.
            bw = null; //κάντον null
            GC.Collect(); //ενεργοποίησε τον garbage collector
        }


        //πεδία
        private int klirwseis; //αριθμός των κληρώσεων που θα γίνουν
        private int stiles; //αριθμός των στηλών για την εκάστοτε κλήρωση
        private int nikites; //αριθμός των νικητών της κάθε κλήρωσης
        private int nikitesSynolo; //αριθμός των νικητών όλων των κληρώσεων
        private int minStilesCount; //ο ελάχιστος αριθμός στηλών σε κάθε κλήρωση
        private int maxStilesCount; //ο μέγιστος αριθμός στηλών σε κάθε κλήρωση
        private Random random = new Random(); //γεννήτρια τυχαίων αριθμών


        //όταν ο χρήστης πατήσει το κουμπί Έναρξη Προσομοίωσης
        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (bw != null) //αν ο worker τρέχει ήδη
            {
                MessageBox.Show("Δεν μπορείς να τρέξεις άλλη ταυτόχρονα!",
                    "Τρέχεις ήδη μια προοσωμοίωση!", MessageBoxButton.OK, MessageBoxImage.Asterisk); //εμφάνισε μήνυμα σφάλματος
                return; //και επέστρεψε
            }

            if (etiKlirosewn.Text == "" && arithmosKlirosewn.Text == "") //αν ο χρήστης δεν συμπήρωσε κανένα από τα πεδία Έτη κληρώσεων ή αριθμός κληρώσεων
            {
                MessageBox.Show("Εισήγαγε είτε τα έτη κληρώσεων, είτε τον αριθμό κληρώσεων για να τρέξει η προσωμοίωση!",
                    "Πρόσεξε!", MessageBoxButton.OK, MessageBoxImage.Exclamation); //εμφάνισε μήνυμα σφάλματος
                return; //επέστρεψε
            }

            if (minStiles.Text == "" && maxStiles.Text == "") //αν ο χρήστης δεν συμπληρώσει κανένα από τα πεδία Ελάχιστος ή Μέγιστος αριθμός στηλών
            {
                //βάλε αυτές τις default τιμές
                minStiles.Text = "2000000";
                maxStiles.Text = "4000000";
            }
            if (minStiles.Text != "" ^ maxStiles.Text != "") //αν ο χρήστης επιλέξει ή το ένα ή το άλλο
            {
                if (minStiles.Text == "") //αν δεν έχει επιλέξει Ελάχιστος αριθμός στηλών
                {
                    minStiles.Text = (int.Parse(maxStiles.Text) - 2000000).ToString(); //κάνε τον ελάχιστο αριθμό στηλών ίσο με το μέγιστο - 2,000,000
                }
                else //αν δεν έχει επιλέξει μέγιστος αριθμός στηλών
                {
                    maxStiles.Text = (int.Parse(minStiles.Text) + 2000000).ToString(); //κάνε τον μέγιστο αριθμό στηλών ίσο με τον ελάχιστο + 2,000,000
                }
            }

            //πάρε το μέγιστο και ελάχιστο αριθμό στηλών σε μεταβλητές
            minStilesCount = int.Parse(minStiles.Text);
            maxStilesCount = int.Parse(maxStiles.Text);

            //ξεκίνα τον worker
            bw = new BackgroundWorker(); //αρχικοποίηση worker
            bw.WorkerReportsProgress = true; //ενεργοποίηση δυνατότητας ενημέρωσης progress
            bw.WorkerSupportsCancellation = true; //ενεργοποίηση δυνατότητας διακοπής worker
            bw.ProgressChanged += bw_ProgressChanged; //event handler για ενημέρωση progress
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted; //event handler για ολοκήρωση εργασίας worker
            bw.DoWork += do_work; //event handler για ενεργοποίηση εργασίας worker

            if (arithmosKlirosewn.Text != "") klirwseis = int.Parse(arithmosKlirosewn.Text); //αν ο αριθμώς κληρώσεων είναι συμπληρωμένος, ο αριθμός κληρώσεων γίνεται ίσος με αυτόν.
            else klirwseis = 104 * int.Parse(etiKlirosewn.Text); //αλλιώς ο αριθμός κληρώσεων θα είναι ίσος με 104*έτη που συμπληρώσαμε.

            progress.Maximum = klirwseis; //το maximum του progress bar θα είναι ο αριθμός των κληρώσεων.

            mesosOros.Text = ""; //καθαρίζουμε το μήνυμα του μέσου όρου πριν ξεκινήσουμε καινούργια προσομοίωση

            bw.RunWorkerAsync(); //ξεκινάμε τον worker
        }

        //όταν ολοκληρωθεί ο worker
        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (etiKlirosewn.Text != "") //αν έχουμε επιλέξει έτη κληρώσεων
            {
                //εμφάνισε τον μέσο όρο των νικητών ανα έτος
                mesosOros.Text = "Ο Μέσος Όρος των νικητών ανα χρόνο είναι " + nikitesSynolo / int.Parse(etiKlirosewn.Text);
            }
            else //αν έχουμε επιλέξει αριθμός κληρώσεων
            {
                //εμφάνισε τον μέσο όρο των νικητών σε αυτές τις κληρώσεις
                mesosOros.Text = "Ο Μέσος Όρος των νικητών στις " + klirwseis + " κληρώσεις είναι " + nikitesSynolo / klirwseis;
            }
        }


        //εδώ γίνεται η δουλεια!!!
        private void do_work(object sender, DoWorkEventArgs e)
        {
            bw.ReportProgress(0, null); //αρχικοποίησε το progress στο 0.
            nikitesSynolo = 0; //αρχικοποίησε το σύνολο των νικητών όλων των κληρώσεων

            for (int klir = 0; klir < klirwseis; klir++) //για κάθε κλήρωση
            {
                nikites = 0; //αρχικοποίησε το σύνολο των νικητών αυτής της κλήρωσης

                //οι στήλες θα είναι ένας τυχαίος αριθμός μεταξύ του μέγιστου
                //και του ελάχιστου αριθμού κληρώσεων που επιλέχθηκαν
                stiles = random.Next(minStilesCount, maxStilesCount);

                int[,] arithmoiPaiktwn = new int[stiles + 1, 6]; //αρχικοποίηση του πίνακα που θα δεχτεί τους αριθμούς των στηλών καθώς και τους νικητήριους αριθμούς

                //===================Οι παίκτες επιλέγουν τους αριθμούς στις στήλες τους!!!========================
                for (int i = 0; i < stiles; i++) //για κάθε στήλη
                {
                    for (int j = 0; j < 5; j++) //για κάθε επιλεγμένο από τον παίκτη αριθμό στο πλέγμα 1-45
                    {
                        arithmoiPaiktwn[i, j] = random.Next(1, 45); //όρισε έναν τυχαίο αριθμό 1-45
                        if (j > 0) //αν δεν είναι ο πρώτος αριθμός που έχεις ορίσει
                        {
                            for (int k = 0; k < j; k++) //για κάθε προηγούμενο από αυτόν αριθμό
                            {
                                if (arithmoiPaiktwn[i, k] == arithmoiPaiktwn[i, j]) //τσέκαρε αν ο παίκτης έχει ξαναεπιλέξει τον αριθμό. αν ναι
                                {
                                    j--; //σβήσε τον αριθμό
                                    break; //και επέλεξε κάποιον άλλο στη θέση του
                                }
                            }
                        }
                    }
                    arithmoiPaiktwn[i, 5] = random.Next(1, 20); //επέλεξε τον αριθμό joker της στήλης από 1-20
                }

                //================================Η Ώρα της Κλήρωσης!!!==========================================
                for (int i = 0; i < 5; i++) //για κάθε έναν από τους 5 αριθμούς στο πλέγμα 1-45
                {
                    arithmoiPaiktwn[stiles, i] = random.Next(1, 45); //στην τελευταία στήλη του πίνακα εισήγαγε τους αριθμούς
                    if (i > 0) //αν δεν είναι ο πρώτος που εισάγεις
                    {
                        for (int k = 0; k < i; k++) //για κάθε προηγούμενο από αυτόν αριθμό
                        {
                            if (arithmoiPaiktwn[stiles, k] == arithmoiPaiktwn[stiles, i]) //τσέκαρε αν τον έχεις ξαναεπιλέξει. αν ναι
                            {
                                i--; //σβήσε τον αριθμό
                                break; //και επέλεξε κάποιον άλλο στη θέση του
                            }
                        }
                    }
                }
                arithmoiPaiktwn[stiles, 5] = random.Next(1, 20); //επέλεξε τον τυχερό αριμό joker από 1-20!!!


                //=====================Για να δούμε τις νικητήριες στήλες!!!====================================
                for (int i = 0; i < stiles; i++) //για κάθε στήλη
                {
                    int countWinNums = 0; //αρχικοποίησε τους αριθμούς που ταιριάζουν στο 0.
                    for (int j = 0; j < 5; j++) //για κάθε έναν από τους αριθμούς του παίκτη
                    {
                        for (int k = 0; k < 5; k++) //και για κάθε έναν από τους νικητήριους αριθμούς
                        {
                            if (arithmoiPaiktwn[i, j] == arithmoiPaiktwn[stiles, k]) //έλεγξε αν ταιριάζει ο αριθμός του παίκτη με κάθε έναν από τους νικητήριους. αν ταιριάζει με κάποιον
                            {
                                countWinNums++; //αύξησε τον μετρητή τον νικητήριων!
                                break; //πήγαινε στον επόμενο αριθμό του παίκτη
                            }
                            else continue; //αν δεν ταιρίαζει έλεγξε με τον επόμενο νικητήριο αριθμό.
                        }
                    }
                    if (countWinNums == 5 && arithmoiPaiktwn[i, 5] == arithmoiPaiktwn[stiles, 5]) //αν ο παίκτης έπιασε και τους 5 νικητήριους αριθμούς ΚΑΙ το τζόκερ.
                    {
                        nikites++; //ΤΖΑΚΠΟΤ!!! βάλτον στους νικητές!!!
                        nikitesSynolo++; //Αύξησε και το σύνολο των νικητών όλων των κληρώσεων
                    }
                }

                //Ας ενημερώσουμε την πρόοδο μέχρι στιγμής
                bw.ReportProgress(klir + 1, new
                { //πρώτο όρισμα το ποσοστό του progress. Δεύτερο όρισμα ένα αντικείμενο για την ενημέρωση του dataGrid.
                    id = klir + 1, //το id της κλήρωσης
                    stiles = stiles, //ο αριθμός των στηλών
                    one = arithmoiPaiktwn[stiles, 0], //ο πρώτος νικητήριος αριθμός
                    two = arithmoiPaiktwn[stiles, 1], //ο δεύτερος νικητήριος αριθμός
                    three = arithmoiPaiktwn[stiles, 2], //ο τρίτος νικητήριος αριθμός
                    four = arithmoiPaiktwn[stiles, 3], //ο τέταρτος νικητήριος αριθμός
                    five = arithmoiPaiktwn[stiles, 4], //ο πέμπτος νικητήριος αριθμός
                    tzoker = arithmoiPaiktwn[stiles, 5], //ο νικητήριος Τζόκερ
                    nikites = nikites //Το πλήθος νικητηρίων στηλών
                });
            }


            KillWorker(); //η δουλειά τελέιωσε. Ας σταματήσουμε τον worker
        }

        //event handler άμα αλλάξουμε το κείμενο στα έτη κληρώσεων
        private void etiKlirosewn_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (etiKlirosewn.Text != "") //αν έχουμε συμπληρώσει τα έτη κληρώσεων
            {
                arithmosKlirosewn.IsEnabled = false; //απενεργοποίησε το πεδίο του αριθμού κληρώσεων
            }
            else //αν είναι κενό
            {
                arithmosKlirosewn.IsEnabled = true; //ξαναενεργοποίησέ το
            }

            stripCharacters(sender as TextBox); //σβήσε χαρακτήρες που δεν είναι αριθμοί
        }

        //event handler άμα αλλάξουμε το κείμενο στον αριθμό κληρώσεων
        private void arithmosKlirosewn_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (arithmosKlirosewn.Text != "") //αν έχουμε συμπληρώσει τον αριθμό κληρώσεων
            {
                etiKlirosewn.IsEnabled = false; //απενεργοποίησε το πεδίο των ετών κληρώσεων
            }
            else //αν είναι κενό
            {
                etiKlirosewn.IsEnabled = true; //ξαναενεργοποίησέ το
            }

            stripCharacters(sender as TextBox); //σβήσε χαρακτήρες που δεν είναι αριθμοί
        }

        //αν πατήσουμε το κουμπί διακοπή
        private void diakopi_Click(object sender, RoutedEventArgs e)
        {
            KillWorker(); //διέκοψε τον worker
        }

        //event handler άμα αλλάξουμε το κείμενο στον ελάχιστο αριθμό στηλών
        private void minStiles_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            stripCharacters(sender as TextBox); //σβήσε χαρακτήρες που δεν είναι αριθμοί
        }

        //event handler άμα αλλάξουμε το κείμενο στον μέγιστο αριθμό στηλών
        private void maxStiles_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            stripCharacters(sender as TextBox); //σβήσε χαρακτήρες που δεν είναι αριθμοί
        }

        //σβήσε χαρακτήρες που δεν είναι αριθμοί
        private void stripCharacters(TextBox myText)
        {
            int result; //βοηθητική μεταβλητή result
            bool isValid = int.TryParse(myText.Text, out result); //έλεγξε αν το κείμενο στο textbox είναι αριθμός
            if (isValid || myText.Text.Length == 0) return; //αν είναι ή είναι κενό επέστρεψε
            //αν δεν είναι αριθμός
            myText.Text = myText.Text.Remove(myText.Text.Length - 1); //σβήσε τον τελευταίο χαρακτήρα 
            myText.SelectionStart = myText.Text.Length; //τοποθέτησε τον κέρσορα στο τέλος του κειμένου
        }
    }
}