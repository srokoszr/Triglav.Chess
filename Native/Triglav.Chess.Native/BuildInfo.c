__declspec(dllexport) int TriglavChess_External_Triglav_IsReleaseBuild(void)
{
#if defined(NDEBUG) && !defined(_DEBUG)
	return 1;
#else
	return 0;
#endif
}
