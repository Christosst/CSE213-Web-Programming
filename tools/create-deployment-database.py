"""Build a populated SQLite database for first deployment, never replacing a file."""
from contextlib import closing
from pathlib import Path
import argparse
import json
import os
import sqlite3
import subprocess

parser = argparse.ArgumentParser()
parser.add_argument('--output', required=True)
args = parser.parse_args()
root = Path(__file__).resolve().parent.parent
project = root / 'Demos/UniversityApi'
source = project / 'SeedData/university-import.json'
output = Path(args.output).resolve()
if output.exists():
    raise SystemExit('The artifact database already exists; use a fresh output path. No file was overwritten.')
output.parent.mkdir(parents=True, exist_ok=True)
subprocess.run(['dotnet', 'build', str(project), '-p:UseAppHost=false'], check=True)
env = os.environ.copy()
env.update({'ASPNETCORE_ENVIRONMENT': 'Production', 'Logging__LogLevel__Default': 'Warning',
            'ConnectionStrings__University': f'Data Source={output};Foreign Keys=True'})
subprocess.run(['dotnet', str(project/'bin/Debug/net10.0/UniversityApi.dll'),
                '--contentRoot', str(project), '--import-data', str(source)], env=env, check=True)
expected = json.loads(source.read_text(encoding='utf-8'))
with closing(sqlite3.connect(output)) as connection:
    connection.execute('PRAGMA wal_checkpoint(TRUNCATE)')
    assert connection.execute('PRAGMA integrity_check').fetchone()[0] == 'ok'
    assert connection.execute('PRAGMA foreign_key_check').fetchall() == []
    for table, records in [('Students', 'Students'), ('Teachers', 'Teachers'),
                           ('Courses', 'Courses'), ('CourseSections', 'Sections'),
                           ('Enrollments', 'Students')]:
        assert connection.execute(f'SELECT COUNT(*) FROM {table}').fetchone()[0] == len(expected[records]), table
print('Created and verified the populated first-deployment SQLite database.')
