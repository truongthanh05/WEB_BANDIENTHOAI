using QL_BANDIENTHOAI.Repository;

namespace QL_BANDIENTHOAI.Services
{
    public class DanhGiaService
    {
        private readonly DanhGiaRepository _repo;

        public DanhGiaService()
        {
            _repo = new DanhGiaRepository();
        }

        public double GetAvgStar(string masp)
            => _repo.GetAvgStar(masp);

        public int GetReviewCount(string masp)
            => _repo.GetReviewCount(masp);
    }
}
