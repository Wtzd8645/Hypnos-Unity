namespace Hypnos.Network
{
    public interface IResponseProducer
    {
        /// <summary>
        /// Convert source to IResponse.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public IResponse Produce(object source);
    }
}