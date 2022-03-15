namespace Xylab.Polygon.Judgement.Daemon
{
    public class DaemonOptions
    {
        public string DOMJUDGE_VERSION { get; set; }

        public string BINDIR { get; set; }

        public string ETCDIR { get; set; }

        public string LIBDIR { get; set; }

        public string LIBJUDGEDIR { get; set; }

        public string LOGDIR { get; set; }

        public string RUNDIR { get; set; }

        public string TMPDIR { get; set; }

        public string JUDGEDIR { get; set; }

        public string CHROOTDIR { get; set; }

        public string CGROUPDIR { get; set; }

        public string RUNUSER { get; set; }

        public string RUNGROUP { get; set; }

        public DaemonOptions()
        {
            DOMJUDGE_VERSION = "7.3.3";

            BINDIR = "/opt/domjudge/judgehost/bin";
            ETCDIR = "/opt/domjudge/judgehost/etc";
            LIBDIR = "/opt/domjudge/judgehost/lib";
            LIBJUDGEDIR = "/opt/domjudge/judgehost/lib/judge";
            LOGDIR = "/opt/domjudge/judgehost/log";
            RUNDIR = "/opt/domjudge/judgehost/run";
            TMPDIR = "/opt/domjudge/judgehost/tmp";
            JUDGEDIR = "/opt/domjudge/judgehost/judgings";
            CHROOTDIR = "/chroot/domjudge";
            CGROUPDIR = "/sys/fs/cgroup";

            RUNUSER = "domjudge-run";
            RUNGROUP = "domjudge-run";
        }
    }
}
