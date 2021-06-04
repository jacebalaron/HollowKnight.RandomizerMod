namespace RandomizerMod.MultiWorld
{
    public interface IMultiWorldCompatibleRandomizer
    {
        /* TODO !
        public MWMenu mwMenu {
            
        };
        
        RandoResult Randomize();
        void StartNewGame(RandoResult);*/
        MultiWorldMenu CreateMultiWorldMenu();

        void SetStartGameButtonVisibility(bool visibility);
    }
}
