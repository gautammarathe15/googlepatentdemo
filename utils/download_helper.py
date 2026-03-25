import time
from pathlib import Path

def wait_for_download(download_dir: str, timeout: int = 60) -> str:
    """
    Waits until a .pdf appears and any temporary .crdownload files disappear.
    Returns path to downloaded file or raises TimeoutError.
    """
    download_dir_path = Path(download_dir)
    end_time = time.time() + timeout
    last_size = -1
    candidate = None

    while time.time() < end_time:
        pdfs = list(download_dir_path.glob("*.pdf"))
        crdownloads = list(download_dir_path.glob("*.crdownload"))

        if pdfs and not crdownloads:
            candidate = max(pdfs, key=lambda p: p.stat().st_mtime)
            size = candidate.stat().st_size
            if size == last_size and size > 0:
                return str(candidate.resolve())
            last_size = size

        time.sleep(0.5)

    raise TimeoutError(f"No completed PDF found in '{download_dir}' after {timeout}s")