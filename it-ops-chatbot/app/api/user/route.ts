import { NextResponse } from 'next/server';
import { exec } from 'child_process';
import { promisify } from 'util';

const execAsync = promisify(exec);

export async function GET() {
  try {
    // First check if user is logged in
    const { stdout: accountInfo } = await execAsync('az account show');
    const account = JSON.parse(accountInfo);

    // Get user details
    const { stdout: userInfo } = await execAsync('az ad signed-in-user show');
    const user = JSON.parse(userInfo);

    return NextResponse.json({
      email: user.userPrincipalName || user.mail || account.user.name
    });
  } catch (error) {
    console.error('Error getting Azure CLI user:', error);
    return NextResponse.json(
      { error: 'Please login using az login first' },
      { status: 401 }
    );
  }
} 