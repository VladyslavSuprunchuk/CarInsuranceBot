namespace CarInsuranceBot.Core.Const
{
    public class OpenaiKeywords
    {
        public const string PolicyInsuranceTemplate = """
                                                     Policy title:

                                                     Policy number:

                                                     Name of insured:

                                                     Address:

                                                     Email ID:

                                                     Policy:

                                                     Period of insurance:

                                                     Limitations As To Use:

                                                     Limits of Liability:

                                                     Registered Office:

                                                     Policy issuing office:
                                                     """;

        public const string FillTemplateRequest = "Fill the following template with random data";
        public const string LiveChattingRequest = "Please execute following request {0}, if it is related to the topic of Identity and Vehicle Records, or any relevant matters, proceed accordingly. If not, just respond that the request is not appropriate.";

        public const string ModelName = "gpt-3.5-turbo-instruct";
    }
}
