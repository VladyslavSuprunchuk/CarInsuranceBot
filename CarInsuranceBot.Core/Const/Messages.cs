namespace CarInsuranceBot.Core.Const
{
    public class Messages
    {
        public const string Welcome = """ 
             Welcome! This bot was made by Vlad S.
             I can help you purchase car insurance.

             Please, submit a photo of your passport and vehicle identification document.
             Add the appropriate caption for photos.
             With keywords 'passport' and 'vehicle card'.

             To confirm, information from documents press 'Confirm data'
             To clerify, information from documents press 'Reshare data'
             """;

        public const string NoPhotoDescription = """ 
             Please, provide photos separately on by one, add the appropriate caption for them.
             With keywords 'passport' and 'vehicle card'.
             """;

        public const string DocumentConfirmed = """ 
             Great!

             The final step is confirmation of payment.
             Please, pay $100 to receive the insurance policy document.

             To confirm, press 'Comfirm payment', or to decline, press 'Decline payment'.
             """;

        public const string PaymentDeclined = """ 
             Sorry

             $100 is the only available price at the moment.
             Please stay tuned for updates on future insurance pricing.
             """;

        public const string InvalidCommand = """ 
             Oops,

             Can't recognize such command.
             Please, send one of the commands given above and follow the instructions.
             """;

        public const string Finish = """ 
             Thank you for using our technology.

             Come back whenever you need to, we look forward to it.
             """;
    }
}
